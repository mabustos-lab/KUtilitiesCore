using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace KUtilitiesCore.Logger.Info
{
    internal class CallerInfo
    {

        public CallerInfo()
        {
            ClassName=string.Empty;
            FileName = "No Info";
            LineNumber = 0;
            MethodSignature = "No Info";
            IsEmpty = true;
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

        public static CallerInfo Empty
        {
            get => new();
        }

        public string ClassName { get; private set; }
        public string FileName { get; private set; }
        public bool IsEmpty { get; private set; }
        public int LineNumber { get; private set; }
        public string MethodSignature { get; private set; }

        public override string ToString()
        {
            return $" At Caller:{ClassName} {MethodSignature} File: {FileName} LN: {LineNumber}";
        }

    }
}