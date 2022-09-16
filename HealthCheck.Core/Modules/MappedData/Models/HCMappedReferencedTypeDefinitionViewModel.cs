namespace HealthCheck.Core.Modules.MappedData.Models
{
    /// <summary></summary>
    public class HCMappedReferencedTypeDefinitionViewModel
	{
		/// <summary>
		/// Internal id.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// How to reference this class from mappings. Defaults to type name without namespace.
		/// </summary>
		public string ReferenceId { get; set; }

		/// <summary>
		/// Display name of this definition.
		/// </summary>
		public string DisplayName { get; set; }

		/// <summary>
		/// Type of the decorated class.
		/// </summary>
		public string TypeName { get; set; }

		/// <summary>
		/// Optional notes to display in the UI.
		/// </summary>
		public string Remarks { get; set; }
	}
}
