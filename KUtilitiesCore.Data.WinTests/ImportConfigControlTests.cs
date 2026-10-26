using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.Win.Importer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KUtilitiesCore.Data.WinTests
{
    /// <summary>
    /// Pruebas de contrato para los controles de configuración de importación:
    /// toda opción modificable desde la UI debe reflejarse en el objeto que
    /// devuelve <see cref="IImportConfigControl.GetParsingOptions"/>, de lo
    /// contrario el lector usa valores por defecto que el usuario nunca pidió.
    /// </summary>
    [TestClass]
    public sealed class ImportConfigControlTests
    {
        [STATestMethod]
        public void ExcelConfig_GetParsingOptions_ShouldReturnSelectedSheet()
        {
            using var control = new ExcelConfigControl();
            control.AddSheetForTesting("Hoja1");
            control.AddSheetForTesting("Hoja2");
            control.SelectSheetForTesting(1);

            var options = (ExcelParsingOptions)control.GetParsingOptions();

            Assert.AreEqual("Hoja2", options.SheetName,
                "La hoja seleccionada en el combo debe ser la que recibe el lector; SelectedText siempre esta vacio en DropDownList.");
        }

        [STATestMethod]
        public void CsvConfig_GetParsingOptions_ShouldExpose_TrimAndEmptyLines()
        {
            using var control = new CsvConfigControl();

            var defaults = (TextFileParsingOptions)control.GetParsingOptions();
            Assert.IsTrue(defaults.TrimValues,
                "El contrato por defecto de TextFileParsingOptions recorta espacios; la UI debe reflejarlo.");
            Assert.IsTrue(defaults.IgnoreEmptyLines,
                "El contrato por defecto de TextFileParsingOptions ignora lineas vacias; la UI debe reflejarlo.");

            control.SetTrimValuesForTesting(false);
            control.SetIgnoreEmptyLinesForTesting(false);

            var modified = (TextFileParsingOptions)control.GetParsingOptions();
            Assert.IsFalse(modified.TrimValues,
                "Desmarcar 'Quitar espacios' debe propagarse a las opciones de parsing.");
            Assert.IsFalse(modified.IgnoreEmptyLines,
                "Desmarcar 'Ignorar lineas vacias' debe propagarse a las opciones de parsing.");
        }

        [STATestMethod]
        public void ExcelConfig_GetParsingOptions_ShouldExpose_RowRange()
        {
            using var control = new ExcelConfigControl();

            control.SetRowRangeForTesting(3, 10);
            var ranged = (ExcelParsingOptions)control.GetParsingOptions();
            Assert.AreEqual(3, ranged.StartRow);
            Assert.AreEqual(10, ranged.EndRow);

            control.SetRowRangeForTesting(1, 0);
            var allRows = (ExcelParsingOptions)control.GetParsingOptions();
            Assert.AreEqual(1, allRows.StartRow);
            Assert.IsNull(allRows.EndRow,
                "EndRow = 0 significa 'leer hasta el final'; debe mapearse a null para respetar el contrato de ExcelParsingOptions.");
        }
    }
}
