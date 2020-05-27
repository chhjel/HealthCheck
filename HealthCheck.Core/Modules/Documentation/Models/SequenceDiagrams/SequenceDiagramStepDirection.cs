namespace HealthCheck.Core.Modules.Documentation.Models.SequenceDiagrams
{
    /// <summary>
    /// Direction to the next step.
    /// </summary>
    public enum SequenceDiagramStepDirection
    {
        /// <summary>
        /// The next step is to the right.
        /// </summary>
        Forward = 0,

        /// <summary>
        /// The next step is to the left.
        /// </summary>
        Backward,

        /// <summary>
        /// The next step is self.
        /// </summary>
        Still
    }
}
