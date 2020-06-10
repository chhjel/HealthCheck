namespace HealthCheck.Module.DynamicCodeExecution.Models
{
    /// <summary>
    /// A static suggestion triggered by entering "@@@.".
    /// </summary>
    public class CodeSuggestion
    {
        /// <summary>
        /// A static suggestion triggered by entering "@@@.".
        /// </summary>
        /// <param name="name">Label shown in the popup suggestion menu.</param>
        /// <param name="description">Description shown in the popup suggestion menu.</param>
        /// <param name="suggestion">
        /// Code to be inserted.
        /// <para>Can contain default values that can be tabbed into. Format: ${number:default value}</para>
        /// <para>Example: GetService(${1:id}, ${2:count})</para>
        /// </param>
        public CodeSuggestion(string name, string description, string suggestion)
        {
            Name = name;
            Description = description;
            Suggestion = suggestion;
        }

        /// <summary>
        /// Label shown in the popup suggestion menu.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description shown in the popup suggestion menu.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Code to be inserted.
        /// <para>Can contain default values that can be tabbed into. Format: ${number:default value}</para>
        /// <para>Example: GetService(${1:id}, ${2:count})</para>
        /// </summary>
        public string Suggestion { get; set; }
    }
}
