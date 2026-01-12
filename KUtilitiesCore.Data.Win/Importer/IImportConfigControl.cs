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
        void Initialize(string fileName);
        IParsingOptions GetParsingOptions();
        event EventHandler OptionsChanged;
    }
}
