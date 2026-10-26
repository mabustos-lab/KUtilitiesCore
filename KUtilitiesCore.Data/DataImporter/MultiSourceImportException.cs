using KUtilitiesCore.Data.Validation.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter
{
    /// <summary>
    /// Representa el fallo de una única fuente dentro de una operación de importación
    /// multi-fuente. Es la unidad de trazabilidad de <see cref="MultiSourceImportException"/>:
    /// identifica la fuente por su nombre y expone la causa del fallo, que es bien una
    /// excepción de lectura (fase de carga) o la colección de errores de validación con
    /// índices de fila relativos a la fuente que falló.
    /// </summary>
    /// <remarks>
    /// Se diseñó como tipo independiente (y no como un par suelto en la excepción) para que
    /// los consumidores puedan inspeccionar cada fuente fallida de forma uniforme sin
    /// conocer la fase en la que se produjo el fallo.
    /// </remarks>
    /// <example>
    /// <code>
    /// var falloLectura = new SourceImportFailure("VentasCSV", new IOException("archivo bloqueado"));
    /// var falloValidacion = new SourceImportFailure("VentasExcel", new[]
    /// {
    ///     new ValidationFailure("Edad", "El valor no es un entero válido.", 4)
    /// });
    /// </code>
    /// </example>
    public sealed class SourceImportFailure
    {
        /// <summary>
        /// Inicializa un fallo cuya causa es una excepción de lectura producida durante
        /// la carga de la fuente.
        /// </summary>
        /// <param name="sourceName">Nombre único de la fuente que falló.</param>
        /// <param name="readException">Excepción que describe la causa de lectura fallida.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourceName"/> es nulo o vacío.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="readException"/> es nulo.
        /// </exception>
        public SourceImportFailure(string sourceName, Exception readException)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentException("El nombre de la fuente no puede estar vacío.", nameof(sourceName));
            ArgumentNullException.ThrowIfNull(readException);

            SourceName = sourceName;
            ReadException = readException;
            ValidationErrors = Array.Empty<ValidationFailure>();
        }

        /// <summary>
        /// Inicializa un fallo cuya causa son errores de validación detectados en los datos
        /// de la fuente. Los índices de fila de cada error son relativos a la propia fuente.
        /// </summary>
        /// <param name="sourceName">Nombre único de la fuente que falló.</param>
        /// <param name="validationErrors">Errores de validación atribuidos a la fuente.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourceName"/> es nulo o vacío, o
        /// <paramref name="validationErrors"/> está vacía.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="validationErrors"/> es nulo.
        /// </exception>
        public SourceImportFailure(string sourceName, IEnumerable<ValidationFailure> validationErrors)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                throw new ArgumentException("El nombre de la fuente no puede estar vacío.", nameof(sourceName));
            ArgumentNullException.ThrowIfNull(validationErrors);

            SourceName = sourceName;
            ReadException = null;
            ValidationErrors = validationErrors.ToArray();
        }

        /// <summary>
        /// Nombre único de la fuente que originó el fallo; coincide con el
        /// <see cref="System.Data.DataTable.TableName"/> asignado a la fuente.
        /// </summary>
        public string SourceName { get; }

        /// <summary>
        /// Excepción de lectura que causó el fallo, o <see langword="null"/> cuando el fallo
        /// se originó en la fase de validación.
        /// </summary>
        public Exception? ReadException { get; }

        /// <summary>
        /// Errores de validación atribuidos a la fuente, con índices de fila relativos a su
        /// tabla de origen. Estará vacía cuando el fallo sea de lectura.
        /// </summary>
        public IReadOnlyList<ValidationFailure> ValidationErrors { get; }
    }

    /// <summary>
    /// Excepción agregada que se lanza cuando una o varias fuentes fallan en una operación
    /// de importación multi-fuente. Expone, para cada fuente fallida, el nombre que la
    /// identifica y la causa del fallo (excepción de lectura o errores de validación con
    /// índice de fila relativo a su fuente).
    /// </summary>
    /// <remarks>
    /// Deriva de <see cref="DataLoadException"/> para que los consumidores existentes que ya
    /// capturan fallos de carga puedan tratar la importación multi-fuente sin cambiar sus
    /// bloques <c>catch</c>, mientras que los nuevos consumidores pueden inspeccionar
    /// <see cref="Failures"/> para reportar exactamente qué fuente falló y por qué.
    /// </remarks>
    /// <example>
    /// <code>
    /// try
    /// {
    ///     using var manager = new MultiSourceImportManager(mapping);
    ///     manager.AddSource(csvReader, "VentasCSV");
    ///     manager.AddSource(excelReader, "VentasExcel");
    ///     var resultado = manager.Import();
    /// }
    /// catch (MultiSourceImportException ex)
    /// {
    ///     foreach (var fallo in ex.Failures)
    ///     {
    ///         Console.WriteLine($"Fuente: {fallo.SourceName}");
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class MultiSourceImportException : DataLoadException
    {
        /// <summary>
        /// Inicializa la excepción con el conjunto completo de fallos detectados durante la
        /// operación. La colección se copia para garantizar que la excepción no muta si el
        /// llamador modifica la secuencia original después de construirla.
        /// </summary>
        /// <param name="message">Mensaje que resume el fallo de la operación multi-fuente.</param>
        /// <param name="failures">Fallos por fuente detectados; nunca debe estar vacía.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="failures"/> es nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="failures"/> está vacía.
        /// </exception>
        public MultiSourceImportException(string message, IEnumerable<SourceImportFailure> failures)
            : base(message)
        {
            ArgumentNullException.ThrowIfNull(failures);
            var copia = failures.ToArray();
            if (copia.Length == 0)
                throw new ArgumentException("Debe indicarse al menos un fallo por fuente.", nameof(failures));

            Failures = copia;
        }

        /// <summary>
        /// Fallos detectados, uno por cada fuente que no superó la operación. Cada elemento
        /// identifica su fuente mediante <see cref="SourceImportFailure.SourceName"/>.
        /// </summary>
        public IReadOnlyList<SourceImportFailure> Failures { get; }
    }
}
