using System;

namespace KUtilitiesCore.Data.Converter.Exceptions
{

    /// <summary>
    /// Representa una excepción que se produce cuando se intenta registrar un convertidor de tipo <see cref="ITypeConverter"/> que ya está registrado.
    /// </summary>
    public class TypeConverterAlreadyRegisteredException : Exception
    {
        #region Constructores

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterAlreadyRegisteredException"/>.
        /// </summary>
        public TypeConverterAlreadyRegisteredException()
            : base()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterAlreadyRegisteredException"/> con un mensaje de error especificado.
        /// </summary>
        /// <param name="message">El mensaje de error que explica la razón de la excepción.</param>
        public TypeConverterAlreadyRegisteredException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterAlreadyRegisteredException"/> con un mensaje de error especificado y una referencia a la excepción interna que es la causa de esta excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que explica la razón de la excepción.</param>
        /// <param name="inner">La excepción que es la causa de la excepción actual, o una referencia nula (Nothing en Visual Basic) si no se especifica una excepción interna.</param>
        public TypeConverterAlreadyRegisteredException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Constructores
    }
}
