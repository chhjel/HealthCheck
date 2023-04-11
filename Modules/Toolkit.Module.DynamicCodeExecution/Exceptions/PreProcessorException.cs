using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.Module.DynamicCodeExecution.Exceptions
{
    /// <summary>
    /// An exception occured in a pre-processor.
    /// </summary>
    [Serializable]
    public class PreProcessorException : Exception
    {
        internal PreProcessorException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected PreProcessorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
