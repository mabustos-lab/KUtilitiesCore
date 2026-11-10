namespace KUtilitiesCore.Data.WPFTests.WPF;

using System.Data;
using KUtilitiesCore.Data.WPF.Importer;

/// <summary>
/// Valida la extracción pura de los errores pintados por el pipeline sobre una
/// <see cref="DataRow"/>: el descriptor solo lee <c>RowError</c>/<c>GetColumnError</c>
/// (única fuente de verdad: <c>ImportManager.SetColumnError</c>) y nunca muta la fila.
/// </summary>
[TestClass]
public sealed class RowErrorDescriptorTests
{
    [TestMethod]
    public void RowErrorDescriptor_MapsColumnErrors_FromDataRow()
    {
        var table = new DataTable();
        table.Columns.Add("Nombre");
        table.Columns.Add("Edad");
        var row = table.Rows.Add("Ana", "abc");
        row.SetColumnError("Edad", "Se esperaba un valor de tipo [Int32] para el valor 'abc'.");
        row.SetColumnError("Nombre", "El campo 'Nombre' es requerido.");
        row.RowError = "La fila contiene problemas generales.";

        var descriptors = RowErrorDescriptor.Describe(row);

        Assert.AreEqual(3, descriptors.Count);
        var edad = descriptors.Single(d => d.ColumnName == "Edad");
        Assert.AreEqual("Se esperaba un valor de tipo [Int32] para el valor 'abc'.", edad.Message);
        var nombre = descriptors.Single(d => d.ColumnName == "Nombre");
        Assert.AreEqual("El campo 'Nombre' es requerido.", nombre.Message);
        var fila = descriptors.Single(d => d.ColumnName.Length == 0);
        Assert.AreEqual("La fila contiene problemas generales.", fila.Message);

        row.ClearErrors();
        Assert.AreEqual(0, RowErrorDescriptor.Describe(row).Count);
    }
}
