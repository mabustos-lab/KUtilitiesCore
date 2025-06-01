using KUtilitiesCore.Extensions;
using System;
using System.Linq;

namespace KUtilitiesCore.Encryption
{
    /// <summary>
    /// Clase que proporciona funcionalidad para generar claves hash únicas basadas en datos de entorno.
    /// </summary>
    public static class HashIdentity
    {
        /// <summary>
        /// Enumeración que define las posibles fuentes de datos de entorno para generar la clave hash.
        /// </summary>
        [Flags]
        public enum IdentitySeed
        {
            /// <summary>
            /// Utiliza el nombre de la máquina como parte de la clave hash.
            /// </summary>
            MachineName = 1,

            /// <summary>
            /// Utiliza el nombre del usuario como parte de la clave hash.
            /// </summary>
            UserName = 2
        }

        /// <summary>
        /// Genera una clave hash única derivada de los datos de entorno especificados.
        /// </summary>
        /// <param name="seed">Las fuentes de datos de entorno a utilizar para generar la clave hash.</param>
        /// <returns>Una cadena que representa la clave hash generada.</returns>
        /// <exception cref="ArgumentException">Se lanza si el parámetro <paramref name="seed"/> no contiene ningún valor válido.</exception>
        public static string GetUniqueDerivedkey(IdentitySeed seed)
        {
            if (seed == 0)
                throw new ArgumentException("El parámetro 'seed' debe contener al menos un valor.", nameof(seed));

            // Obtiene las claves basadas en las banderas especificadas en el parámetro 'seed'.
            var keys = Enum.GetValues(typeof(IdentitySeed))
                           .Cast<IdentitySeed>()
                           .Where(flag => seed.HasFlag(flag) && flag != 0)
                           .Select(flag =>
                           {
                               return flag switch
                               {
                                   IdentitySeed.MachineName => Environment.MachineName,
                                   IdentitySeed.UserName => Environment.UserName,
                                   _ => null
                               };
                           })
                           .Where(key => key != null)
                           .ToList();

            // Genera el hash utilizando un servicio de hash.
            IHashService hashService = FactoryEncryptionService.GetHashServise(false, 32);
            return hashService.Hash(string.Join("-", keys));
        }
    }
}
