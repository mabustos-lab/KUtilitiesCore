using System;

namespace KUtilitiesCore.Data.Converter.Exceptions
{
    /// <summary>
    /// Representa una excepción que se produce cuando se usar convertidor de tipo <see cref="ITypeConverter"/> que no está registrado.
    /// </summary>
    public class TypeConverterNotRegisteredException : Exception
    {
        #region Constructors
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterNotRegisteredException"/>.
        /// </summary>
        public TypeConverterNotRegisteredException()
            : base()
        {
        }
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterNotRegisteredException"/> con un mensaje de error especificado.
        /// </summary>
        /// <param name="message">El mensaje de error que explica la razón de la excepción.</param>
        public TypeConverterNotRegisteredException(string message)
            : base(message)
        {
        }
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TypeConverterNotRegisteredException"/> con un mensaje de error especificado y una referencia a la excepción interna que es la causa de esta excepción.
        /// </summary>
        /// <param name="message">El mensaje de error que explica la razón de la excepción.</param>
        /// <param name="inner">La excepción que es la causa de la excepción actual, o una referencia nula (Nothing en Visual Basic) si no se especifica una excepción interna.</param>
        public TypeConverterNotRegisteredException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Constructors
    }
}