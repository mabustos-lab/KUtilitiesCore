using KUtilitiesCore.Data.DataImporter;
using KUtilitiesCore.Data.Validation.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KUtilitiesCore.Data.DataImporter.Tests
{
    [TestClass()]
    public class MultiSourceImportExceptionTests
    {
        [TestMethod(DisplayName = "MultiSourceImportException Deriva De DataLoadException")]
        public void MultiSourceImportException_DerivaDe_DataLoadException()
        {
            var exception = new MultiSourceImportException(
                "mensaje", new[] { CreateReadFailure("FuenteCSV") });

            Assert.IsInstanceOfType(exception, typeof(DataLoadException));
        }

        [TestMethod(DisplayName = "MultiSourceImportException Expone Fallos Por Fuente")]
        public void MultiSourceImportException_ExponeFallosPorFuente()
        {
            var failures = new List<SourceImportFailure>
            {
                CreateReadFailure("FuenteCSV"),
                CreateValidationFailure("FuenteExcel")
            };

            var exception = new MultiSourceImportException("mensaje", failures);

            Assert.AreEqual(2, exception.Failures.Count);
            CollectionAssert.AreEquivalent(
                new[] { "FuenteCSV", "FuenteExcel" },
                exception.Failures.Select(f => f.SourceName).ToList());
        }

        [TestMethod(DisplayName = "MultiSourceImportException Inmutabilidad De La Coleccion De Fallos")]
        public void MultiSourceImportException_ColeccionDeFallosEsInmutable()
        {
            var failures = new List<SourceImportFailure>
            {
                CreateReadFailure("FuenteCSV")
            };
            var exception = new MultiSourceImportException("mensaje", failures);
            failures.Add(CreateValidationFailure("FuenteExcel"));

            Assert.AreEqual(1, exception.Failures.Count);
        }

        [TestMethod(DisplayName = "Fallo De Lectura Lleva Nombre De Fuente Y Excepcion De Causa")]
        public void FalloDeLectura_LlevaNombreDeFuenteYExcepcionDeCausa()
        {
            var cause = new IOException("no se pudo abrir el archivo");
            var failure = new SourceImportFailure("FuenteCSV", cause);

            Assert.AreEqual("FuenteCSV", failure.SourceName);
            Assert.AreSame(cause, failure.ReadException);
            Assert.IsNotNull(failure.ValidationErrors);
            Assert.AreEqual(0, failure.ValidationErrors.Count);
        }

        [TestMethod(DisplayName = "Fallo De Validacion Lleva Nombre De Fuente Y Errores Con Indice Relativo")]
        public void FalloDeValidacion_LlevaNombreDeFuenteYErroresConIndiceRelativo()
        {
            var errors = new[]
            {
                new ValidationFailure("Edad", "El valor no es un entero válido.", 3),
                new ValidationFailure("Edad", "El valor no es un entero válido.", 7)
            };
            var failure = new SourceImportFailure("FuenteExcel", errors);

            Assert.AreEqual("FuenteExcel", failure.SourceName);
            Assert.IsNull(failure.ReadException);
            Assert.AreEqual(2, failure.ValidationErrors.Count);
            Assert.AreEqual(3, failure.ValidationErrors[0].IndexRow);
            Assert.AreEqual(7, failure.ValidationErrors[1].IndexRow);
        }

        private static SourceImportFailure CreateReadFailure(string sourceName)
            => new(sourceName, new IOException("error de lectura simulado"));

        private static SourceImportFailure CreateValidationFailure(string sourceName)
            => new(sourceName, new[]
            {
                new ValidationFailure("Edad", "El valor no es un entero válido.", 1)
            });
    }
}
