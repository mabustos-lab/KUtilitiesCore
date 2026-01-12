using KUtilitiesCore.Data.Validation;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCoreTests.Data.Validation
{
    [TestClass]
    public class RuleBuilderTest
    {
        [TestMethod]
        public void RuleBuilderForUserIsValid()
        {
            var usuarioValido = new Usuario { Nombre = "Juan Pérez", Email = "juan.perez@email.com", Edad = 30, Genero = "Masculino" };
            var usuarioValidator = new UsuarioValidator();
            Assert.IsTrue(ValidateAndPrint(usuarioValidator, usuarioValido, "Usuario Válido"));
        }
        [TestMethod]
        public void RuleBuilderForUserIsNotValid()
        {
            var usuarioInvalido = new Usuario { Nombre = "A", Email = "email-invalido", Edad = -5, Genero = "Otro" };
            var usuarioValidator = new UsuarioValidator();
            Assert.IsFalse(ValidateAndPrint(usuarioValidator, usuarioInvalido, "Usuario Válido"));
        }
        [TestMethod]
        public void RuleBuilderForUserIsNotValidEdadExcesiva()
        {
            var usuarioEdadExcesiva = new Usuario { Nombre = "Matusalen", Email = "matusalen@eden.com", Edad = 999, Genero = "M" };
            var usuarioValidator = new UsuarioValidator();
            Assert.IsFalse(ValidateAndPrint(usuarioValidator, usuarioEdadExcesiva, "Usuario Válido"));
        }
        [TestMethod]
        public void RuleBuilderForPeriodoIsValid()
        {
            var periodoValido = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(1), Descripcion = "Periodo correcto" };
            var periodoValidator = new PeriodoValidator();
            Assert.IsTrue(ValidateAndPrint(periodoValidator, periodoValido, "Fechas válidas"));
        }
     
        [TestMethod]
        public void RuleBuilderForPeriodoIsNotValidDate()
        {
            var periodoInvalidoFechas = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(-1), Descripcion = "Fechas incorrectas" };
            var periodoValidator = new PeriodoValidator();
            Assert.IsFalse(ValidateAndPrint(periodoValidator, periodoInvalidoFechas, "Fechas no válidas"));
        }
        [TestMethod]
        public void RuleBuilderForPeriodoIsNotValidDescription()
        {
            var periodoInvalidoDesc = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(1), Descripcion = "Corto" };
            var periodoValidator = new PeriodoValidator();
            Assert.IsFalse(ValidateAndPrint(periodoValidator, periodoInvalidoDesc, "Descripción no válida"));
        }

        private static bool ValidateAndPrint<T>(IValidator<T> validator, T instance, string description)
        {
            Console.WriteLine($"\nValidando: {description}");
            var result = validator.Validate(instance);

            if (result.IsValid)
            {
                Debug.WriteLine("Resultado: Válido");
            }
            else
            {
                Debug.WriteLine("Resultado: Inválido");
                foreach (var failure in result.Errors)
                {
                    Debug.WriteLine(failure.ToString());
                }
            }
            return result.IsValid;
        }
        // --- Definición de Validadores ---

        /// <summary>
        /// Validador para la clase Periodo.
        /// Demuestra validación condicional a nivel de objeto.
        /// </summary>
        public class PeriodoValidator : AbstractValidator<Periodo>
        {
            public PeriodoValidator()
            {
                // Regla para la propiedad Descripcion
                RuleFor(p => p.Descripcion).NotEmpty().Length(6, 100);

                // Regla para FechaInicio (opcional, podría ser nula)
                // RuleFor(p => p.FechaInicio).NotNull(); // Si fuera requerida

                // Regla para FechaFin (opcional, podría ser nula)
                RuleFor(p => p.FechaFin).NotNull(); // Si fuera requerida

                // Regla personalizada a nivel de objeto para validar la relación entre fechas
                Custom((periodo, context, failures) =>
                {
                    // Solo validar si ambas fechas tienen valor
                    if (periodo.FechaInicio.HasValue && periodo.FechaFin.HasValue)
                    {
                        if (periodo.FechaFin.Value < periodo.FechaInicio.Value)
                        {
                            // Añadir fallo específico. No se asocia a una propiedad concreta,
                            // o se puede asociar a una de ellas (ej: FechaFin).
                            failures.Add(new ValidationFailure(nameof(Periodo.FechaFin),
                                "La fecha de fin debe ser posterior o igual a la fecha de inicio.",-1,
                                periodo.FechaFin.Value));
                        }
                    }
                    // Se podrían añadir más validaciones complejas aquí
                });
            }
        }
        /// <summary>
        /// Validador para la clase Usuario.
        /// Demuestra el uso de IAllowedValue y otras reglas comunes con WithMessage y placeholders.
        /// </summary>
        public class UsuarioValidator : AbstractValidator<Usuario>
        {
            // Definición de valores permitidos para Género
            private static readonly IRuleAllowedValue<string> GeneroAllowedValues =
                new RuleAllowedStringValues(["Masculino", "Femenino", "M", "F"], StringComparison.OrdinalIgnoreCase);

            public UsuarioValidator()
            {
                RuleFor(u => u.Nombre)
                    .NotEmpty().WithMessage("El nombre de usuario ('{PropertyName}') es obligatorio.") // Placeholder {PropertyName}
                    .Length(3, 50).WithMessage("El nombre '{PropertyName}' debe tener entre {MinLength} y {MaxLength} caracteres. Valor: '{PropertyValue}'."); // Placeholders {PropertyName}, {MinLength}, {MaxLength}, {PropertyValue}

                RuleFor(u => u.Email)
                    .NotEmpty().WithMessage("El email es requerido.") // Mensaje simple
                    .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    .WithMessage("El formato del email '{PropertyValue}' no es válido para '{PropertyName}'."); // Placeholders {PropertyValue}, {PropertyName}

                RuleFor(u => u.Edad)
                    .GreaterThan(0).WithMessage("La edad '{PropertyName}' debe ser mayor que {ComparisonValue}.") // Placeholder {ComparisonValue}
                    .LessThan(120).WithMessage("La edad '{PropertyValue}' para '{PropertyName}' debe ser menor que {ComparisonValue}."); // Placeholders {PropertyValue}, {PropertyName}, {ComparisonValue}

                // Uso de la regla AllowedValues con mensaje personalizado usando placeholders
                RuleFor(u => u.Genero)
                    .NotEmpty().WithMessage("El campo '{PropertyName}' es obligatorio.") // Placeholder {PropertyName}
                    .AllowedValues(GeneroAllowedValues)
                    // Mensaje personalizado que usa placeholders específicos de AllowedValuesValidator y generales
                    .WithMessage("El valor '{PropertyValue}' no es válido para '{PropertyName}'. Permitidos: {AllowedValuesDescription}."); // Placeholders {PropertyValue}, {PropertyName}, {AllowedValuesDescription}
            }
        }
        public class Periodo
        {
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaFin { get; set; }
            public string Descripcion { get; set; }
        }

        public class Usuario
        {
            public string Nombre { get; set; }
            public string Email { get; set; }
            public int Edad { get; set; }
            public string Genero { get; set; } // Se validará contra AllowedStringValues
        }
    }
}
