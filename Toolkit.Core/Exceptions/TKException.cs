using System;
using System.Runtime.Serialization;

namespace QoDL.Toolkit.Core.Exceptions
{
    /// <summary>
    /// Generic exception thrown from Toolkit code when no other more specific exception is implemented yet.
    /// </summary>
    [Serializable]
    public class TKException : Exception
    {
        /// <summary>
        /// Generic exception thrown from Toolkit code.
        /// </summary>
        public TKException() { }

        /// <summary>
        /// Initializes a new instance of the object with serialized data.
        /// </summary>
        protected TKException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Generic exception thrown from Toolkit code.
        /// </summary>
        public TKException(string message) : base(message) { }
    }
}
