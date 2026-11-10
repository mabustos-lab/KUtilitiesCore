using KUtilitiesCore.Data.Wizard.Configuration;

namespace KUtilitiesCore.Data.Win.Importer
{
    /// <summary>
    /// Vista WinForms del contrato compartido de controles de configuración de
    /// importación. Se conserva como interfaz propia del ensamblado para no romper
    /// la superficie pública existente, pero no agrega miembros: hereda íntegramente
    /// de <see cref="KUtilitiesCore.Data.Wizard.Configuration.IImportConfigControl"/>
    /// para que el asistente de WinForms y el de WPF hospeden los controles bajo el
    /// mismo contrato, sin duplicar la especificación de cada miembro.
    /// </summary>
    public interface IImportConfigControl : KUtilitiesCore.Data.Wizard.Configuration.IImportConfigControl
    {
    }
}
