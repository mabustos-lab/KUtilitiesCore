namespace KUtilitiesCore.Diagnostics.Logger
{
    /// <summary>
    /// Proporciona una fábrica para gestionar múltiples servicios de registro (<see cref="ILoggerService"/>).
    /// Permite registrar, obtener y verificar la existencia de servicios de registro específicos.
    /// </summary>
    public interface ILogFactory : ILoggerService
    {
        #region Methods

        /// <summary>
        /// Verifica si un servicio de registro del tipo especificado está registrado.
        /// </summary>
        /// <typeparam name="Tlog">El tipo del servicio de registro a verificar.</typeparam>
        /// <returns>True si el servicio de registro está registrado; de lo contrario, False.</returns>
        bool Contains<Tlog>() where Tlog : ILoggerService;

        /// <summary>
        /// Obtiene una instancia del servicio de registro del tipo especificado.
        /// </summary>
        /// <typeparam name="Tlog">El tipo del servicio de registro a obtener.</typeparam>
        /// <returns>Una instancia del servicio de registro solicitado.</returns>
        Tlog GetLogService<Tlog>() where Tlog : ILoggerService;

        /// <summary>
        /// Registra un nuevo servicio de registro.
        /// </summary>
        /// <typeparam name="Tlog">El tipo del servicio de registro a registrar.</typeparam>
        /// <param name="logger">La instancia del servicio de registro a registrar.</param>
        void RegisterLogger<Tlog>(Tlog logger) where Tlog : ILoggerService;

        /// <summary>
        /// Elimina el registro de un servicio de registro del tipo especificado.
        /// </summary>
        /// <typeparam name="Tlog">El tipo del servicio de registro a eliminar.</typeparam>
        /// <returns>True si el servicio de registro fue eliminado exitosamente; de lo contrario, False.</returns>
        bool UnRegisterLogger<Tlog>() where Tlog : ILoggerService;

        #endregion Methods
    }
}