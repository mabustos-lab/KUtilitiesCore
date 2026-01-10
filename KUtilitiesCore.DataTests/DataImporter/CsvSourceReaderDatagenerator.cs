using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.DataTests.DataImporter
{
    internal static class CsvSourceReaderDatagenerator
    {
        /// <summary>
        /// Clage que genera datos para pruebas
        /// </summary>
        public static string RootPath { get; set; }

        public static string BasicDataPath => Path.Combine(RootPath, "datos_basicos.csv");
        public static string DuplicatedDataPath => Path.Combine(RootPath, "datos_duplicados.csv");
        public static string EmptyDataPath => Path.Combine(RootPath, "datos_vacios.csv");
        public static string TabSplitDataPath => Path.Combine(RootPath, "datos_tabulado.csv");
        public static string MappingDataPath => Path.Combine(RootPath, "datos_DiferenteColumna.csv");

        public static void ClearFilesTest()
        {
            File.Delete(BasicDataPath);
            File.Delete(DuplicatedDataPath);
            File.Delete(EmptyDataPath);
            File.Delete(TabSplitDataPath);
            File.Delete(MappingDataPath);
        }
        public static void CreateDataTest()
        {
            CreateBasicData();
            CreateDiferentMapingData();
            CreateDuplicatedColumnData();
            CreateDiferentSplitCharData();
            CreateEmptyDataData();
        }
        static void CreateBasicData()
        {
            File.WriteAllText(BasicDataPath,
            "Nombre,Apellido,Edad,Ciudad,Profesion\n" +
            "Juan,Perez,25,Madrid,Ingeniero\n" +
            "Maria,Gomez,30,Barcelona,Medico\n" +
            "Carlos,Lopez,35,Valencia,Profesor\n" +
            "Ana,Rodriguez,28,Sevilla,Abogado");
        }
        static void CreateDiferentMapingData()
        {
            File.WriteAllText(MappingDataPath,
            "Nombre,Apellidos,Edad,Ciudad,Profesion\n" +
            "Juan,Perez,25,Madrid,Ingeniero\n" +
            "Maria,Gomez,30,Barcelona,Medico\n" +
            "Carlos,Lopez,35,Valencia,Profesor\n" +
            "Ana,Rodriguez,28,Sevilla,Abogado");
        }
        static void CreateDuplicatedColumnData()
        {
            File.WriteAllText(DuplicatedDataPath,
           "Nombre,Edad,Nombre,Ciudad,Edad,Ciudad\n" +
           "Juan,25,Juan Duplicado,Madrid,30,Madrid Duplicado\n" +
           "Maria,30,Maria Duplicada,Barcelona,35,Barcelona Duplicada\n" +
           "Carlos,35,Carlos Duplicado,Valencia,40,Valencia Duplicada");
        }
        static void CreateDiferentSplitCharData()
        {
            File.WriteAllText(TabSplitDataPath,
            "Nombre\tApellido\tEdad\tCargo\n" +
            "Juan\tPerez\t25\tDesarrollador\n" +
            "Maria\tGomez\t30\tDiseñadora\n" +
            "Carlos\tLopez\t35\tGerente");
        }
        static void CreateEmptyDataData()
        {
            File.WriteAllText(EmptyDataPath,
           "Nombre,Apellido,Edad,Ciudad,Email\n" +
           "Juan,Perez,25,Madrid,juan@email.com\n" +
           "Maria,Gomez,,Barcelona,\n" +
           "Carlos,,35,Valencia,carlos@email.com\n" +
           ",Martinez,28,Sevilla,laura@email.com");
        }
    }
}
