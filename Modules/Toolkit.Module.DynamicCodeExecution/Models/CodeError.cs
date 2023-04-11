namespace QoDL.Toolkit.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// Represents an error that occured.
    /// </summary>
    public class CodeError
    {
        /// <summary>
        /// Line where the error occured or -1 if not relevant.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Column where the error occured or -1 if not relevant.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Error text.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// New code error with the given message.
        /// </summary>
        public CodeError(string message) : this(-1, -1, message) {}

        /// <summary>
        /// New code error with the given line and message.
        /// </summary>
        public CodeError(int line, int column, string message)
        {
            Line = line;
            Column = column;
            Message = message;
        }
    }
}
