using KUtilitiesCore.Data.Converter.Abstracts;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KUtilitiesCore.Data.Converter.Types
{
    internal class DateTimeConverter : NonNullableConverter<DateTime>
    {
        #region Fields

        private readonly string dateTimeFormat;
        private readonly DateTimeStyles dateTimeStyles;
        private readonly IFormatProvider formatProvider;

        #endregion Fields

        #region Constructors

        public DateTimeConverter()
            : this(string.Empty)
        {
        }

        public DateTimeConverter(string dateTimeFormat)
            : this(dateTimeFormat, CultureInfo.InvariantCulture)
        {
        }

        public DateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider)
            : this(dateTimeFormat, formatProvider, DateTimeStyles.None)
        {
        }

        public DateTimeConverter(string dateTimeFormat, IFormatProvider formatProvider, DateTimeStyles dateTimeStyles)
        {
            this.dateTimeFormat = dateTimeFormat;
            this.formatProvider = formatProvider;
            this.dateTimeStyles = dateTimeStyles;
        }

        protected override bool InternalConvert(string value, out DateTime result)
        {
            //Valido si viene en el formato de excel formato OLE Automation
            if (double.TryParse(value, out _))
            {
                if (ReadDateFromExcel(value, out result))
                    return true;
            }
            if (string.IsNullOrWhiteSpace(dateTimeFormat))
            {
                return DateTime.TryParse(value, out result);
            }

            return DateTime.TryParseExact(value, dateTimeFormat, formatProvider, dateTimeStyles, out result);
        }

        private static bool ReadDateFromExcel(string value, out DateTime result)
        {
            result = DateTime.MinValue;
            if (double.TryParse(value, out double oaDate))
            {
                DateTime tmp =  DateTime.FromOADate(oaDate);
                string dateFromXL = tmp.ToString("MM/dd/yyyy");
                Regex dateRegex = new Regex(@"^([1-9]|0[1-9]|1[0-2])[- / .]([1-9]|0[1-9]|1[0-9]|2[0-9]|3[0-1])[- / .](1[9][0-9][0-9]|2[0][0-9][0-9])$");
                if (!dateRegex.IsMatch(dateFromXL)) throw new FormatException($"La fecha no tiene el formato correcto (OLE Automation Date). Valor: [{value}]");
                result = tmp;
                return true;
            }
            return false;
        }

        #endregion Methods
    }
}