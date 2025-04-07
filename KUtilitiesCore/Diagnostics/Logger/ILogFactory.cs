namespace KUtilitiesCore.Diagnostics.Logger
{
    public interface ILogFactory : ILoggerService
    {
        #region Methods

        bool Contains<Tlog>() where Tlog : ILoggerService;

        void RegisterLogger<Tlog>(Tlog logger) where Tlog : ILoggerService;

        bool UnRegisterLogger<Tlog>() where Tlog : ILoggerService;

        #endregion Methods
    }
}