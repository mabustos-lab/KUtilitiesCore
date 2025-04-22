using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.MVVM.Helpers
{
    public class ViewModelSourceException : Exception
    {
        public ViewModelSourceException()
        {
        }

        public ViewModelSourceException(string message) : base(message)
        {
        }
    }
}
