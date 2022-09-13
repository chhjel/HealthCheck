namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary>
    /// Left/right pair of members from two different mapping definitions.
    /// </summary>
    public class HCMappedMemberDefinitionPair
	{
		/// <summary>
		/// The left side member being mapped.
		/// </summary>
		public HCMappedMemberDefinition[] Left { get; set; }

		/// <summary>
		/// The right side member being mapped.
		/// </summary>
		public HCMappedMemberDefinition[] Right { get; set; }
	}
}
