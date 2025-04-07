using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace KUtilitiesCore.Diagnostics.Exceptions
{
    internal class CallerInfo
    {
        #region Constructors

        public CallerInfo()
        {
        }

        public CallerInfo(StackFrame frame) : this(
            (frame.GetMethod() as MethodInfo)?.DeclaringType?.FullName ?? string.Empty,
            (frame.GetMethod() as MethodInfo)?.ToShortString() ?? string.Empty,
                frame.GetFileName(),
                frame.GetFileLineNumber())
        {
        }

        public CallerInfo(string ClasName, string methodSignature, string fileName, int lineNumber)
        {
            FileName = string.IsNullOrEmpty(fileName) ? string.Empty : Path.GetFileName(fileName);
            LineNumber = lineNumber;
            ClassName = ClasName;
            MethodSignature = methodSignature;
            IsEmpty = false;
        }

        #endregion Constructors

        #region Properties

        public static CallerInfo Empty
        {
            get => new CallerInfo()
            {
                FileName = "No Info",
                LineNumber = 0,
                MethodSignature = "No Info",
                IsEmpty = true
            };
        }

        public string ClassName { get; private set; }
        public string FileName { get; private set; }
        public bool IsEmpty { get; private set; }
        public int LineNumber { get; private set; }
        public string MethodSignature { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $" At Caller:{ClassName} {MethodSignature} File: {FileName} LN: {LineNumber}";
        }

        #endregion Methods
    }
}