using System.Diagnostics;
using System.Text;

namespace KUtilitiesCore.Diagnostics.Exceptions
{
    /// <summary>
    /// Extrae la información de la excepción recibida.
    /// </summary>
    public class ExceptionInfo
    {
        #region Fields

        private const int indentWidth = 4;
        private readonly Func<int, int, string> ident = (depth, nestedDepth) => new string(' ', indentWidth * (depth + nestedDepth));
        private int exceptionDepth;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ExceptionInfo"/>.
        /// </summary>
        public ExceptionInfo()
        {
            AggregateExceptions = new List<ExceptionInfo>();
            AditionaInfo = new List<KeyValuePair<string, string>>();
            exceptionDepth = 1;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ExceptionInfo"/> con una excepción especificada.
        /// </summary>
        /// <param name="ex">La excepción a cargar.</param>
        /// <param name="deep">La profundidad de la excepción.</param>
        public ExceptionInfo(Exception ex, int deep = 1) : this()
        {
            exceptionDepth = deep;
            LoadException(ex, deep);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Obtiene la información adicional de la excepción.
        /// </summary>
        public List<KeyValuePair<string, string>> AditionaInfo { get; private set; }

        /// <summary>
        /// Obtiene las excepciones agregadas.
        /// </summary>
        public List<ExceptionInfo> AggregateExceptions { get; private set; }

        /// <summary>
        /// Obtiene el identificador de la excepción.
        /// </summary>
        public string ExceptionId { get; private set; }

        /// <summary>
        /// Obtiene el enlace de ayuda de la excepción.
        /// </summary>
        public string HelpLink { get; private set; } = string.Empty;

        /// <summary>
        /// Obtiene la excepción interna.
        /// </summary>
        public ExceptionInfo InnerException { get; private set; }

        /// <summary>
        /// Obtiene el mensaje de la excepción base.
        /// </summary>
        public string MessageBaseException { get; private set; }

        /// <summary>
        /// Obtiene el mensaje de la excepción.
        /// </summary>
        public string MessageException { get; private set; }

        /// <summary>
        /// Obtiene la fuente de la excepción.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Obtiene el método de destino donde ocurrió la excepción.
        /// </summary>
        public string TargetSite { get; private set; }

        /// <summary>
        /// Obtiene el tipo de la excepción.
        /// </summary>
        public string TypeException { get; private set; }

        /// <summary>
        /// Obtiene la información de la excepción base.
        /// </summary>
        internal CallerInfo BaseException { get; private set; }

        /// <summary>
        /// Obtiene la traza de la pila de la excepción.
        /// </summary>
        internal List<CallerInfo> StackTraceException { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Obtiene el bloque de información adicional.
        /// </summary>
        /// <returns>El bloque de información adicional como una cadena.</returns>
        public string GetAditionalInfoBlock()
        {
            if (AditionaInfo == null || !AditionaInfo.Any()) return string.Empty;
            var block = new StringBuilder();
            block.Append(ident(1, exceptionDepth)).AppendLine("Aditional Info:");
            foreach (var item in AditionaInfo)
            {
                block.Append(ident(2, exceptionDepth)).AppendLine($"{item.Key} => {item.Value}");
            }
            return block.ToString();
        }

        /// <summary>
        /// Obtiene el bloque de enlace de ayuda.
        /// </summary>
        /// <returns>El bloque de enlace de ayuda como una cadena.</returns>
        public string GetHelpLinkBlock()
        {
            if (string.IsNullOrEmpty(HelpLink)) return string.Empty;
            var block = new StringBuilder();
            block.Append(ident(1, exceptionDepth)).AppendLine($"HelpLink: {HelpLink}");
            return block.ToString();
        }

        /// <summary>
        /// Obtiene el reporte de la excepción.
        /// </summary>
        /// <param name="includeStackTrace">Indica si se debe incluir la traza de la pila.</param>
        /// <param name="includeFlatterExceptions">Indica si se deben incluir las excepciones agregadas.</param>
        /// <returns>El reporte de la excepción como una cadena.</returns>
        public string GetReport(bool includeStackTrace = true, bool includeFlatterExceptions = true)
        {
            var block = new StringBuilder();
            block.Append(GetTitleBlock());

            if (!string.IsNullOrEmpty(HelpLink)) block.Append(GetHelpLinkBlock());
            if (AditionaInfo != null && AditionaInfo.Any()) block.Append(GetAditionalInfoBlock());
            block.AppendLine();

            if (includeStackTrace && StackTraceException != null && StackTraceException.Any()) block.Append(GetStackTraceBlock());

            var current = InnerException;
            if (current != null) block.Append(ident(1, exceptionDepth)).AppendLine("InnerException");
            while (current != null)
            {
                block.Append(current.GetReport(includeStackTrace, includeFlatterExceptions));
                current = current.InnerException;
            }

            if (includeFlatterExceptions && AggregateExceptions.Any())
            {
                block.Append(ident(1, exceptionDepth)).AppendLine("AggregateExceptions");
                foreach (var ex in AggregateExceptions)
                {
                    block.Append(ex.GetReport(includeStackTrace, includeFlatterExceptions));
                }
            }

            return block.ToString();
        }

        /// <summary>
        /// Obtiene el bloque de traza de la pila.
        /// </summary>
        /// <returns>El bloque de traza de la pila como una cadena.</returns>
        public string GetStackTraceBlock()
        {
            if (StackTraceException == null || !StackTraceException.Any()) return string.Empty;
            var block = new StringBuilder();
            block.Append(ident(1, exceptionDepth)).AppendLine("StackTrace: ");
            foreach (var st in StackTraceException)
            {
                block.Append(ident(2, exceptionDepth)).AppendLine("\t" + st);
            }
            return block.ToString();
        }

        /// <summary>
        /// Obtiene el bloque de título.
        /// </summary>
        /// <returns>El bloque de título como una cadena.</returns>
        public string GetTitleBlock()
        {
            var block = new StringBuilder();
            block.Append(ident(0, exceptionDepth)).AppendLine($"{TypeException}: \"{MessageException}\"");
            if (!BaseException.IsEmpty)
            {
                block.Append(ident(1, exceptionDepth)).AppendLine("Source:");
                block.Append(ident(2, exceptionDepth)).AppendLine("\t" + BaseException.ToString());
            }
            return block.ToString();
        }

        /// <summary>
        /// Carga la información adicional de la excepción.
        /// </summary>
        /// <param name="ex">La excepción de la cual cargar la información adicional.</param>
        private void LoadAditionalInfo(Exception ex)
        {
            foreach (var item in ex.Data.Keys)
            {
                AditionaInfo.Add(new KeyValuePair<string, string>(item.ToString(), ex.Data[item]?.ToString()));
            }
        }

        /// <summary>
        /// Carga las excepciones agregadas de una excepción de tipo <see cref="AggregateException"/>.
        /// </summary>
        /// <param name="ex">La excepción agregada a cargar.</param>
        private void LoadAggregataException(AggregateException ex)
        {
            foreach (var innerEx in ex.InnerExceptions)
            {
                AggregateExceptions.Add(new ExceptionInfo(innerEx, exceptionDepth));
            }
        }

        /// <summary>
        /// Carga la información de la excepción.
        /// </summary>
        /// <param name="ex">La excepción a cargar.</param>
        /// <param name="deep">La profundidad de la excepción.</param>
        private void LoadException(Exception ex, int deep)
        {
            if (ex is AggregateException aggregateException)
            {
                LoadAggregataException(aggregateException);
            }

            HelpLink = ex.HelpLink ?? string.Empty;
            MessageException = ex.Message;
            Source = ex.Source ?? string.Empty;
            TargetSite = ex.TargetSite?.ToString() ?? string.Empty;
            TypeException = ex.GetType().Name;
            InnerException = ex.InnerException != null ? new ExceptionInfo(ex.InnerException, deep + 1) : null;
            ExceptionId = ex.GetExceptionHash();
            LoadExceptionBase(ex.GetBaseException());
            LoadStacktrace(ex);
            LoadAditionalInfo(ex);
        }

        /// <summary>
        /// Carga la información de la excepción base.
        /// </summary>
        /// <param name="ex">La excepción base a cargar.</param>
        private void LoadExceptionBase(Exception ex)
        {
            BaseException = CallerInfo.Empty;
            if (ex == null) return;
            MessageBaseException = ex.Message;
            var st = new StackTrace(ex, true);
            var frame = st.GetFrame(0);
            if (frame != null) BaseException = new CallerInfo(frame);
        }

        /// <summary>
        /// Carga la traza de la pila de la excepción.
        /// </summary>
        /// <param name="ex">La excepción de la cual cargar la traza de la pila.</param>
        private void LoadStacktrace(Exception ex)
        {
            var st = new StackTrace(ex, true);
            StackTraceException = st.GetFrames()?.Select(x => new CallerInfo(x)).Reverse().ToList();
        }

        #endregion Methods
    }
}