using KUtilitiesCore.Data.DataImporter.Interfaces;
using System.Data;

namespace KUtilitiesCore.Data.WPFTests.TestDoubles;

/// <summary>
/// Lector de fuente de datos falso para pruebas del orquestador del asistente.
/// </summary>
/// <remarks>
/// Permite inyectar una tabla muestra, contar las lecturas realizadas y, mediante
/// una puerta <see cref="TaskCompletionSource{TResult}"/>, mantener la lectura
/// pendiente para probar la anti-reentrada de la carga.
/// </remarks>
internal sealed class FakeDataSourceReader : IDataSourceReader
{
    private readonly TaskCompletionSource<DataTable>? _gate;

    public FakeDataSourceReader(DataTable table)
        : this(table, null)
    {
    }

    public FakeDataSourceReader(DataTable table, TaskCompletionSource<DataTable>? gate)
    {
        Table = table;
        _gate = gate;
    }

    public DataTable Table { get; }

    public int ReadCalls { get; private set; }

    public Exception? ThrowOnRead { get; set; }

    public bool CanRead => ThrowOnRead is null;

    public DataTable ReadData()
    {
        ReadCalls++;
        if (ThrowOnRead is not null)
        {
            throw ThrowOnRead;
        }

        return Table;
    }

    public Task<DataTable> ReadDataAsync()
    {
        ReadCalls++;
        if (ThrowOnRead is not null)
        {
            return Task.FromException<DataTable>(ThrowOnRead);
        }

        return _gate?.Task ?? Task.FromResult(Table);
    }
}
