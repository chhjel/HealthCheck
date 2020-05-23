﻿using System;

namespace HealthCheck.DynamicCodeExecution.Exceptions
{
    /// <summary>
    /// An exception occured in a pre-processor.
    /// </summary>
    internal class PreProcessorException : Exception
    {
        internal PreProcessorException(string message, Exception inner) : base(message, inner) {}
    }
}
