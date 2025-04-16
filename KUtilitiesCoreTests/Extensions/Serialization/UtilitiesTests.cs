using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUtilitiesCore.Extensions.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace KUtilitiesCore.Extensions.Serialization.Tests
{
    [TestClass()]
    public class UtilitiesTests
    {
        [TestMethod()]
        public void ToJsonTest()
        {
            Persona persona = CreateObject();
            string jsonPersona= persona.ToJson();
            Assert.IsTrue(jsonPersona.Length>0);
        }

        [TestMethod()]
        public void FromJsonTest()
        {
            Persona persona = CreateObject();
            string jsonPersona = persona.ToJson();
            Persona personaFromJson= jsonPersona.FromJson<Persona>();
            Assert.IsTrue(persona.GetHashCode()== personaFromJson.GetHashCode());
        }

        [TestMethod()]
        public void ToXmlTest()
        {
            Persona persona = CreateObject();
            string jsonPersona = persona.ToXml();
            Assert.IsTrue(jsonPersona.Length > 0);
        }

        [TestMethod()]
        public void FromXmlTest()
        {
            Persona persona = CreateObject();
            string jsonPersona = persona.ToXml();
            Persona personaFromJson = jsonPersona.FromXml<Persona>();
            Assert.IsTrue(persona.GetHashCode() == personaFromJson.GetHashCode());
        }
        private Persona CreateObject()
        {
            return new Persona
            {
                Id = 1,
                Nombre = "Juan Pérez",
                Edad = 30,
                FechaRegistro = DateTime.UtcNow,
                Direccion = new Direccion { Calle = "Calle Falsa 123", Ciudad = "Springfield" },
                Hobbies = new List<string> { "Leer", "Programar", "Viajar" }
            };
        }
        // --- Clase de Ejemplo para Pruebas ---
        public class Persona
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public int Edad { get; set; }
            public DateTime FechaRegistro { get; set; }
            public Direccion Direccion { get; set; } // Propiedad anidada
            public List<string> Hobbies { get; set; } // Lista

            public Persona() // Constructor sin parámetros necesario para XmlSerializer
            {
                Hobbies = new List<string>();
            }
            public override int GetHashCode()
            {
                return $"{Id}{Nombre}{Edad}{FechaRegistro}".GetHashCode();
            }
        }
        public class Direccion
        {
            public string Calle { get; set; }
            public string Ciudad { get; set; }
        }
    }
}