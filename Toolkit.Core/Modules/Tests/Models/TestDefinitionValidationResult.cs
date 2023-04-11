namespace QoDL.Toolkit.Core.Modules.Tests.Models
{
    /// <summary>
    /// Result from <see cref="TestDefinition.Validate"/>.
    /// </summary>
    public class TestDefinitionValidationResult
    {
        /// <summary>
        /// Initialize a new <see cref="TestDefinitionValidationResult"/>.
        /// </summary>
        public TestDefinitionValidationResult(TestDefinition testDefinition)
        {
            Test = testDefinition;
        }

        /// <summary>
        /// Related test definition.
        /// </summary>
        public TestDefinition Test { get; set; }

        /// <summary>
        /// True if there is no error.
        /// </summary>
        public bool IsValid => string.IsNullOrWhiteSpace(Error);

        /// <summary>
        /// Error if any,
        /// </summary>
        public string Error { get; set; }
    }
}
