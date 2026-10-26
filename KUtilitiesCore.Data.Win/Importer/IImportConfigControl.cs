using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Win.Importer
{
    /// <summary>
    /// Contrato de los controles de configuración de importación que el
    /// <see cref="ImportWizardForm"/> hospeda dinámicamente según el tipo de archivo
    /// seleccionado (CSV/Texto o Excel). Cada implementación expone las opciones de
    /// análisis que el asistente pasa a la fuente de datos correspondiente.
    /// </summary>
    public interface IImportConfigControl
    {
        /// <summary>
        /// Inicializa el contron a partir de la información del archivo.
        /// </summary>
        void Initialize(string fileName);

        /// <summary>
        /// Construye las opciones de análisis a partir del estado actual del control.
        /// </summary>
        /// <returns>Instancia de <see cref="IParsingOptions"/> lista para la fuente de datos.</returns>
        IParsingOptions GetParsingOptions();

        /// <summary>
        /// Notifica que el usuario modificó alguna opción del control, permitiendo al
        /// asistente recalcular la configuración antes de cargar los datos.
        /// </summary>
        event EventHandler? OptionsChanged;
    }
}
