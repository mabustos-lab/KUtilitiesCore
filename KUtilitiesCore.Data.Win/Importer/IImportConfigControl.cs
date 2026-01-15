using KUtilitiesCore.Data.DataImporter.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Data.Win.Importer
{
    public interface IImportConfigControl
    {
        /// <summary>
        /// Inicializa el contron a partir de la información del archivo.
        /// </summary>
        void Initialize(string fileName);
        IParsingOptions GetParsingOptions();
        event EventHandler OptionsChanged;
    }
}
