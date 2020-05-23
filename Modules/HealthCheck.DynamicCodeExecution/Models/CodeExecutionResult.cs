using System.Collections.Generic;

namespace HealthCheck.DynamicCodeExecution.Models
{
    /// <summary>
    /// Result of a code execution.
    /// </summary>
    public class CodeExecutionResult
    {
        /// <summary>
        /// Result type.
        /// </summary>
        public enum StatusTypes
        {
            /// <summary>
            /// Code was executed without any errors.
            /// </summary>
            Executed = 0,

            /// <summary>
            /// A compiler error occured.
            /// </summary>
            CompilerError,

            /// <summary>
            /// A runtime error occured.
            /// </summary>
            RuntimeError
        }

        /// <summary>
        /// Source code after any post-processing.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Execution result.
        /// </summary>
        public StatusTypes Status { get; set; }

        /// <summary>
        /// Output from the Main method.
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// List of any errors that occured.
        /// </summary>
        public List<CodeError> Errors { get; set; } = new List<CodeError>();

        /// <summary>
        /// List of any dumps from any of the Dump extension methods.
        /// </summary>
        public List<DataDump> Dumps { get; set; } = new List<DataDump>();

        /// <summary>
        /// List of any diffs from any of the Diff extension methods.
        /// </summary>
        public List<DiffModel> Diffs { get; set; } = new List<DiffModel>();

        /// <summary>
        /// List with ids of the pre-processors that was applied to the code.
        /// </summary>
        public List<string> AppliedPreProcessorIds { get; set; } = new List<string>();

        /// <summary>
        /// Returns (<see cref="int"/>)<see cref="Status"/>.
        /// </summary>
        public int StatusCode => (int)Status;

        /// <summary>
        /// Returns <see cref="Status"/>.ToString()
        /// </summary>
        public string StatusString => Status.ToString();
    }
}
