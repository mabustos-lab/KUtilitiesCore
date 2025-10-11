using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace KUtilitiesCore.Dal.ConnectionBuilder
{
    /// <summary>
    /// Define el contrato para una cadena de conexión a base de datos, incluyendo la propia cadena
    /// de conexión y el nombre del proveedor asociado.
    /// </summary>
    /// <remarks>
    /// Esta interfaz suele ser implementada por clases que encapsulan la construcción o gestión de
    /// cadenas de conexión a bases de datos. Proporciona acceso a la cadena de conexión resultante
    /// y al nombre del proveedor de la base de datos.
    /// </remarks>
    /// Defines the contract for a database connection string, including the connection string
    /// itself and the associated provider name.
    public interface IConnectionString
    {

        /// <summary>
        /// Establece o obtiene la cadena de conexión resultante de los parámetros de la clase.
        /// </summary>
        string CnnString { get; }

        /// <summary>
        /// Establece o obtiene el nombre del proveedor de la conexión a la base de datos.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        string ProviderName { get; set; }

    }
}