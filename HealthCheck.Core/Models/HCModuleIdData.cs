namespace HealthCheck.Core.Models
{
    /// <summary>
    /// Contains id and name.
    /// </summary>
    public class HCModuleIdData
	{
		/// <summary>
		/// Id of something.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of something.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Name and id.
		/// </summary>
		public override string ToString() => $"\"{Name}\" ({Id})";
    }
}
