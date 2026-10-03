using KUtilitiesCore.Data.DataImporter.Interfaces;
using KUtilitiesCore.Data.ImportDefinition;
using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Orquestador de importación multi-fuente: procesa en una sola operación transaccional
    /// (todo-o-nada) N fuentes que comparten un mismo esquema (<see cref="FieldDefinitionCollection"/>),
    /// entregando un <see cref="DataSet"/> con un <see cref="DataTable"/> por fuente, identificado
    /// con el nombre de su origen.
    /// </summary>
    /// <remarks>
    /// Compone (no modifica) el pipeline existente: crea internamente un <see cref="ImportManager"/>
    /// por fuente con una copia propia del esquema, de modo que cada fuente conserva sus índices de
    /// fila y sus errores de validación de forma relativa y aislada. Si cualquier fuente falla al
    /// leerse o al validarse, la operación completa falla con <see cref="MultiSourceImportException"/>
    /// reportando todas las fuentes fallidas, sin exponer resultados parciales.
    /// </remarks>
    /// <example>
    /// <code>
    /// using var manager = new MultiSourceImportManager(mapping);
    /// manager.AddSource(csvReader, "VentasCSV");
    /// manager.AddSource(excelReader, "VentasExcel");
    /// try
    /// {
    ///     DataSet resultado = manager.Import();
    /// }
    /// catch (MultiSourceImportException ex)
    /// {
    ///     foreach (var fallo in ex.Failures)
    ///     {
    ///         Console.WriteLine($"Fuente fallida: {fallo.SourceName}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class MultiSourceImportManager : IDisposable
    {
        private readonly FieldDefinitionCollection _fieldDefinitions;
        private readonly List<RegisteredSource> _sources = new();
        private DataSet? _lastResult;
        private bool _disposed;

        /// <summary>
        /// Inicializa el orquestador con el esquema común que compartirán todas las fuentes
        /// registradas. El esquema se clona por fuente (ver <see cref="AddSource"/>), por lo que
        /// mutaciones posteriores de las copias internas no interfieren entre fuentes.
        /// </summary>
        /// <param name="fieldDefinitions">Definición de columnas común a todas las fuentes.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fieldDefinitions"/> es nulo.
        /// </exception>
        public MultiSourceImportManager(FieldDefinitionCollection fieldDefinitions)
        {
            ArgumentNullException.ThrowIfNull(fieldDefinitions);
            _fieldDefinitions = fieldDefinitions;
        }

        /// <summary>
        /// Cantidad de fuentes registradas hasta el momento.
        /// </summary>
        public int SourceCount => _sources.Count;

        /// <summary>
        /// Registra una fuente como el par lector + nombre de origen. El <paramref name="sourceName"/>
        /// es la clave de trazabilidad de la fuente: será el <see cref="DataTable.TableName"/> del
        /// resultado y el identificador en <see cref="MultiSourceImportException"/>.
        /// </summary>
        /// <param name="reader">Lector de datos de la fuente.</param>
        /// <param name="sourceName">Nombre único de la fuente (insensible a mayúsculas, igual que los
        /// nombres de tabla de un <see cref="DataSet"/>).</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="reader"/> es nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourceName"/> es nulo, vacío o ya existe una fuente registrada con ese nombre.
        /// </exception>
        public void AddSource(IDataSourceReader reader, string sourceName)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(reader);
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                throw new ArgumentException(
                    "El nombre de la fuente no puede ser nulo ni estar vacío.", nameof(sourceName));
            }

            if (_sources.Any(s => string.Equals(s.SourceName, sourceName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(
                    $"Ya existe una fuente registrada con el nombre '{sourceName}'.", nameof(sourceName));
            }

            _sources.Add(new RegisteredSource(reader, sourceName, CreateManager()));
        }

        /// <summary>
        /// Ejecuta la operación multi-fuente en dos fases: procesa y valida todas las fuentes y,
        /// solo si todas pasan, ensambla el <see cref="DataSet"/> resultado. Si alguna fuente falla,
        /// lanza <see cref="MultiSourceImportException"/> con todas las fuentes fallidas y no expone
        /// ningún resultado parcial.
        /// </summary>
        /// <returns>
        /// <see cref="DataSet"/> con una copia del <see cref="DataTable"/> de cada fuente,
        /// identificada con su nombre de origen.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// No hay fuentes registradas o el esquema común no tiene definiciones de columnas.
        /// </exception>
        /// <exception cref="MultiSourceImportException">
        /// Una o varias fuentes fallaron al leerse o al validarse.
        /// </exception>
        public DataSet Import()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureCanOperate();

            var failures = ProcessSources();
            if (failures.Count > 0)
            {
                ResetInternalState();
                throw new MultiSourceImportException(
                    "La importación multi-fuente falló: una o varias fuentes no superaron el proceso. " +
                    "No se expone ningún resultado parcial.", failures);
            }

            return AssembleResult();
        }

        /// <summary>
        /// Variante asíncrona de <see cref="Import"/>: procesa las fuentes de forma secuencial
        /// mediante <see cref="IDataSourceReader.ReadDataAsync"/>, comprobando el
        /// <paramref name="cancellationToken"/> entre fuentes para permitir abortar la operación
        /// de forma cooperativa. La cancelación lanza <see cref="OperationCanceledException"/>
        /// sin exponer resultados parciales y dejando el gestor en el mismo estado de antes de
        /// la operación, de modo que puede reintentarse.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token de cancelación cooperativa: se respeta antes de procesar cada fuente.
        /// </param>
        /// <returns>
        /// <see cref="DataSet"/> con una copia del <see cref="DataTable"/> de cada fuente,
        /// identificada con su nombre de origen.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// No hay fuentes registradas o el esquema común no tiene definiciones de columnas.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// El <paramref name="cancellationToken"/> se canceló durante la operación.
        /// </exception>
        /// <exception cref="MultiSourceImportException">
        /// Una o varias fuentes fallaron al leerse o al validarse.
        /// </exception>
        public async Task<DataSet> ImportAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureCanOperate();

            List<SourceImportFailure> failures;
            try
            {
                failures = await ProcessSourcesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                ResetInternalState();
                throw;
            }

            if (failures.Count > 0)
            {
                ResetInternalState();
                throw new MultiSourceImportException(
                    "La importación multi-fuente falló: una o varias fuentes no superaron el proceso. " +
                    "No se expone ningún resultado parcial.", failures);
            }

            return AssembleResult();
        }

        /// <summary>
        /// Libera los <see cref="ImportManager"/> internos de todas las fuentes registradas y
        /// el último resultado ensamblado.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            foreach (var source in _sources)
            {
                source.Manager.Dispose();
            }

            _lastResult?.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Valida las precondiciones de la operación: al menos una fuente registrada y un esquema
        /// común con definiciones de columnas. Se rechaza con error de precondición antes de
        /// procesar cualquier fuente.
        /// </summary>
        private void EnsureCanOperate()
        {
            if (_sources.Count == 0)
            {
                throw new InvalidOperationException(
                    "No hay fuentes registradas para importar.");
            }

            if (_fieldDefinitions.Count == 0)
            {
                throw new InvalidOperationException(
                    "El esquema común no tiene definiciones de columnas configuradas.");
            }
        }

        /// <summary>
        /// Fase 1: procesa (lee y valida) todas las fuentes, coleccionando los fallos por fuente
        /// sin interrumpir el procesamiento, para poder reportar todas las fuentes fallidas en una
        /// única excepción agregada.
        /// </summary>
        private List<SourceImportFailure> ProcessSources()
        {
            var failures = new List<SourceImportFailure>();
            foreach (var source in _sources)
            {
                try
                {
                    source.Manager.LoadData(source.Reader);
                }
                catch (Exception ex)
                {
                    failures.Add(new SourceImportFailure(source.SourceName, ex));
                    continue;
                }

                if (!source.Manager.ValidateDataTypes())
                {
                    failures.Add(new SourceImportFailure(
                        source.SourceName,
                        source.Manager.ValidationErrors.Errors.OfType<ValidationFailure>()));
                }
            }

            return failures;
        }

        /// <summary>
        /// Fase 1 asíncrona: procesa (lee y valida) las fuentes de forma secuencial mediante
        /// <see cref="IDataSourceReader.ReadDataAsync"/>, comprobando el token de cancelación antes
        /// de cada fuente y coleccionando los fallos por fuente. A diferencia del camino síncrono,
        /// los errores de lectura los envuelve el propio orquestador como <see cref="DataLoadException"/>
        /// porque la lectura asíncrona no pasa por <see cref="ImportManager.LoadData"/>.
        /// </summary>
        private async Task<List<SourceImportFailure>> ProcessSourcesAsync(CancellationToken cancellationToken)
        {
            var failures = new List<SourceImportFailure>();
            foreach (var source in _sources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                DataTable rawData;
                try
                {
                    rawData = await source.Reader.ReadDataAsync().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    failures.Add(new SourceImportFailure(source.SourceName,
                        new DataLoadException($"Error al leer la fuente '{source.SourceName}'.", ex)));
                    continue;
                }

                source.Manager.ReadData(rawData);
                if (!source.Manager.ValidateDataTypes())
                {
                    failures.Add(new SourceImportFailure(
                        source.SourceName,
                        source.Manager.ValidationErrors.Errors.OfType<ValidationFailure>()));
                }
            }

            return failures;
        }

        /// <summary>
        /// Fase 2: ensambla el <see cref="DataSet"/> resultado con una copia del
        /// <see cref="DataTable"/> de cada fuente, identificándolo con su nombre de origen para
        /// garantizar la trazabilidad. Solo se invoca cuando todas las fuentes pasaron la fase 1.
        /// </summary>
        private DataSet AssembleResult()
        {
            var result = new DataSet();
            foreach (var source in _sources)
            {
                var table = source.Manager.DataSource.Copy();
                table.TableName = source.SourceName;
                result.Tables.Add(table);
            }

            _lastResult = result;
            return result;
        }

        /// <summary>
        /// Descarta el estado intermedio tras un fallo: recrea los <see cref="ImportManager"/>
        /// internos con copias nuevas del esquema, dejando el gestor en el mismo estado que
        /// antes de la operación fallida.
        /// </summary>
        private void ResetInternalState()
        {
            foreach (var source in _sources)
            {
                source.ReplaceManager(CreateManager());
            }
        }

        /// <summary>
        /// Crea el <see cref="ImportManager"/> interno de una fuente con una copia propia del
        /// esquema común, evitando que fuentes distintas compartan una colección mutable.
        /// </summary>
        private ImportManager CreateManager()
        {
            var manager = new ImportManager();
            manager.SetMapping((FieldDefinitionCollection)_fieldDefinitions.Clone());
            return manager;
        }

        /// <summary>
        /// Par lector + nombre de origen registrado, junto con su <see cref="ImportManager"/> interno.
        /// </summary>
        private sealed class RegisteredSource
        {
            public RegisteredSource(IDataSourceReader reader, string sourceName, ImportManager manager)
            {
                Reader = reader;
                SourceName = sourceName;
                Manager = manager;
            }

            public IDataSourceReader Reader { get; }

            public string SourceName { get; }

            public ImportManager Manager { get; private set; }

            /// <summary>
            /// Sustituye el <see cref="ImportManager"/> interno (previamente dispuesto) por una
            /// instancia nueva, usada para descartar el estado intermedio tras un fallo.
            /// </summary>
            public void ReplaceManager(ImportManager newManager)
            {
                Manager.Dispose();
                Manager = newManager;
            }
        }
    }
}
