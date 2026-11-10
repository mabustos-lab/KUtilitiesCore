using KUtilitiesCore.Data.DataExporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Globalization;

namespace KUtilitiesCore.Data.DataExporter.Tests
{
    /// <summary>
    /// Contrato de la preparación compartida del diálogo de exportación: validación del
    /// origen, nombre por defecto con sello temporal y resolución extensión→formato.
    /// Los fachadas de WinForms y WPF deben delegar en esta preparación para garantizar
    /// paridad de comportamiento entre plataformas.
    /// </summary>
    [TestClass]
    public sealed class ExportDialogPreparationTests
    {
        [TestMethod]
        public void ValidateSource_NullOrEmptyTable_ReturnsFalse()
        {
            // Un origen nulo o sin filas no es exportable: la fachada debe poder
            // rechazarlo ANTES de abrir el diálogo, sin acoplarse a la UI.
            Assert.IsFalse(ExportDialogPreparation.ValidateSource(null),
                "Un origen null no debe considerarse exportable.");
            var empty = new DataTable();
            empty.Columns.Add("Col", typeof(string));
            Assert.IsFalse(ExportDialogPreparation.ValidateSource(empty),
                "Un origen con solo encabezados (cero filas) no debe considerarse exportable.");
            var withRows = new DataTable();
            withRows.Columns.Add("Col", typeof(string));
            withRows.Rows.Add("valor");
            Assert.IsTrue(ExportDialogPreparation.ValidateSource(withRows),
                "Un origen con filas sí debe considerarse exportable.");
        }

        [TestMethod]
        public void GetDefaultFileName_FollowsTimestampPattern()
        {
            // El nombre por defecto permite al usuario reconocer la exportación por
            // su fecha de creación: prefijo fijo + sello yyyyMMdd_HHmmss.
            var name = ExportDialogPreparation.GetDefaultFileName();
            StringAssert.StartsWith(name, "Exportacion_");
            var stamp = name["Exportacion_".Length..];
            Assert.AreEqual(15, stamp.Length,
                "El sello debe tener exactamente el largo de yyyyMMdd_HHmmss (8 + 1 + 6).");
            Assert.IsTrue(
                DateTime.TryParseExact(stamp, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _),
                "El sello debe ser parseable como yyyyMMdd_HHmmss.");
        }

        [TestMethod]
        public void ResolveFormat_CsvTsvPsvXlsxXls_MapsToExporter()
        {
            // La resolución decide a cuál exportador delegar: texto delimitado (CSV/TSV/PSV)
            // o Excel (XLSX/XLS 97-2003).
            Assert.AreEqual(ExportFormat.Text, ExportDialogPreparation.ResolveFormat("datos.csv"));
            Assert.AreEqual(ExportFormat.Text, ExportDialogPreparation.ResolveFormat("datos.tsv"));
            Assert.AreEqual(ExportFormat.Text, ExportDialogPreparation.ResolveFormat("datos.psv"));
            Assert.AreEqual(ExportFormat.Excel, ExportDialogPreparation.ResolveFormat("datos.xlsx"));
            Assert.AreEqual(ExportFormat.Excel, ExportDialogPreparation.ResolveFormat("datos.xls"));

            // El exportador de texto necesita además el separador que corresponde a la extensión.
            Assert.AreEqual(",", ExportDialogPreparation.GetSeparator("datos.csv"));
            Assert.AreEqual("\t", ExportDialogPreparation.GetSeparator("datos.tsv"));
            Assert.AreEqual("|", ExportDialogPreparation.GetSeparator("datos.psv"));
        }

        [TestMethod]
        public void ResolveFormat_UnknownExtension_ReturnsNone()
        {
            // Una extensión no soportada (o su ausencia) no debe mapear a ningún
            // exportador: la fachada informa al usuario sin lanzar excepciones aquí.
            Assert.AreEqual(ExportFormat.None, ExportDialogPreparation.ResolveFormat("datos.docx"));
            Assert.AreEqual(ExportFormat.None, ExportDialogPreparation.ResolveFormat("datos"));
            Assert.AreEqual(ExportFormat.None, ExportDialogPreparation.ResolveFormat(""));
        }
    }
}
