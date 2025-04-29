using KUtilitiesCore.Data.Validation;
using KUtilitiesCore.Data.Validation.Core;
using KUtilitiesCore.Data.Validation.RuleValues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCoreTests.Data.Validation
{
    [TestClass]
    public class RuleBuilderTest
    {
        [TestMethod]
        public void RuleBuilderForPeriodoIsValid()
        {
            var periodoValido = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(1), Descripcion = "Periodo correcto" };
            var periodoInvalidoFechas = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(-1), Descripcion = "Fechas incorrectas" };
            var periodoInvalidoDesc = new Periodo { FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddDays(1), Descripcion = "Corto" };

            var periodoValidator = new PeriodoValidator();

            ValidateAndPrint(periodoValidator, periodoValido, "Periodo Válido");
            ValidateAndPrint(periodoValidator, periodoInvalidoFechas, "Periodo Inválido (Fechas)");
            ValidateAndPrint(periodoValidator, periodoInvalidoDesc, "Periodo Inválido (Descripción)");


        }
        private static void ValidateAndPrint<T>(IValidator<T> validator, T instance, string description)
        {
            Console.WriteLine($"\nValidando: {description}");
            var result = validator.Validate(instance);

            if (result.IsValid)
            {
                Console.WriteLine("Resultado: Válido");
            }
            else
            {
                Console.WriteLine("Resultado: Inválido");
                foreach (var failure in result.Errors)
                {
                    Console.WriteLine($" - Propiedad: {failure.PropertyName ?? "<Objeto>"}, Error: {failure.ErrorMessage}, Valor: {failure.AttemptedValue ?? "<null>"}");
                }
            }
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
                RuleFor(p => p.Descripcion).NotEmpty().Length(5, 100);

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
                                "La fecha de fin debe ser posterior o igual a la fecha de inicio.",
                                periodo.FechaFin.Value));
                        }
                    }
                    // Se podrían añadir más validaciones complejas aquí
                });
            }
        }
        ///// <summary>
        ///// Validador para la clase Usuario.
        ///// Demuestra el uso de IAllowedValue y otras reglas comunes.
        ///// </summary>
        //public class UsuarioValidator : AbstractValidator<Usuario>
        //{
        //    // Definición de valores permitidos para Género
        //    private static readonly IRuleAllowedValue<string> GeneroAllowedValues =
        //        new RuleAllowedStringValues(new[] { "Masculino", "Femenino", "M", "F" }, StringComparison.OrdinalIgnoreCase);

        //    public UsuarioValidator()
        //    {
        //        RuleFor(u => u.Nombre)
        //            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.") // Mensaje personalizado
        //            .Length(3, 50).WithMessage("El nombre debe tener entre {MinLength} y {MaxLength} caracteres."); // Placeholders de FluentValidation

        //        RuleFor(u => u.Email)
        //            .NotEmpty()
        //            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase) // Email básico
        //            .WithMessage("El formato del email no es válido.");

        //        RuleFor(u => u.Edad)
        //            .GreaterThan(0).WithMessage("La edad debe ser un número positivo.")
        //            .LessThan(120).WithMessage("La edad parece excesiva.");

        //        // Uso de la regla AllowedValues con la definición creada antes
        //        RuleFor(u => u.Genero)
        //            .NotEmpty().WithMessage("El género es obligatorio.")
        //            .AllowedValues(GeneroAllowedValues)
        //            .WithMessage("El género debe ser {AllowedValuesDescription}."); // Placeholder personalizado
        //                                                                            // Nota: El mensaje por defecto de AllowedValuesValidator ya es descriptivo.
        //                                                                            // WithMessage aquí sobreescribiría ese mensaje.
        //                                                                            // Para usar placeholders como {AllowedValuesDescription}, necesitaríamos
        //                                                                            // extender el formateo de mensajes.
        //    }
        //}
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
