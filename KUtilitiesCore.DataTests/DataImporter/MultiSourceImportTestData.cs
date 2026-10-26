using System;

namespace KUtilitiesCore.DataTests.DataImporter
{
    /// <summary>
    /// Genera los archivos CSV de prueba para las pruebas del orquestador multi-fuente.
    /// Los archivos cubren los escenarios del spec: datos válidos, columnas reordenadas,
    /// un valor de tipo inválido y una ruta de archivo inexistente.
    /// </summary>
    internal static class MultiSourceImportTestData
    {
        public static string RootPath { get; set; } = string.Empty;

        public static string ValidDataPath => Path.Combine(RootPath, "multifuente_validos.csv");
        public static string ReorderedDataPath => Path.Combine(RootPath, "multifuente_reordenados.csv");
        public static string InvalidEdadDataPath => Path.Combine(RootPath, "multifuente_edad_invalida.csv");
        public static string MissingDataPath => Path.Combine(RootPath, "multifuente_inexistente.csv");

        public static void ClearFiles()
        {
            File.Delete(ValidDataPath);
            File.Delete(ReorderedDataPath);
            File.Delete(InvalidEdadDataPath);
        }

        public static void CreateFiles()
        {
            File.WriteAllText(ValidDataPath,
                "Nombre,Apellido,Edad,Ciudad,Profesion\n" +
                "Juan,Perez,25,Madrid,Ingeniero\n" +
                "Maria,Gomez,30,Barcelona,Medico\n" +
                "Carlos,Lopez,35,Valencia,Profesor\n" +
                "Ana,Rodriguez,28,Sevilla,Abogado");

            File.WriteAllText(ReorderedDataPath,
                "Edad,Profesion,Nombre,Ciudad,Apellido\n" +
                "22,Disenador,Pedro,Bilbao,Ruiz\n" +
                "27,Comercial,Lucia,Zaragoza,Navarro");

            File.WriteAllText(InvalidEdadDataPath,
                "Nombre,Apellido,Edad,Ciudad,Profesion\n" +
                "Juan,Perez,25,Madrid,Ingeniero\n" +
                "Maria,Gomez,abc,Barcelona,Medico\n" +
                "Carlos,Lopez,35,Valencia,Profesor");
        }
    }
}
