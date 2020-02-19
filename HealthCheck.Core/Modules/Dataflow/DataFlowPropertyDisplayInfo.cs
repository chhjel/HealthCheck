namespace HealthCheck.Core.Modules.Dataflow
{
    /// <summary>
    /// Options for a property on an entry coming from a dataflow source.
    /// </summary>
    public class DataFlowPropertyDisplayInfo
    {
        /// <summary>
        /// Name of the property to target.
        /// </summary>
        public string PropertyName { get; set; }
        
        /// <summary>
        /// Name of the property to display.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Order of this property. Lower = earlier.
        /// </summary>
        public int UIOrder { get; set; }

        /// <summary>
        /// Hide the property in the UI.
        /// </summary>
        public bool Hidden { get; set; }
    }
}
