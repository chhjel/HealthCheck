using System;

namespace HealthCheck.Core.Modules.MappedData.Attributes
{
    /// <summary>
    /// Decorate a class with this for it to be discovered by HC.
    /// <para>At least 1 property must be decorated with <see cref="HCMappedPropertyAttribute"/>.</para>
    /// <para>Required on at least one side of the mapping.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class HCMappedClassAttribute : Attribute
	{
        /// <summary>
        /// Other class type this class is mapped to.
        /// </summary>
        public Type MappedToType { get; set; }

		/// <summary>
		/// Optionally override display name of this mapping.
		/// </summary>
		public string OverrideName { get; set; }

		/// <summary>
		/// Optional name of the source of the data in this class. Visible in the UI.
		/// </summary>
		public string DataSourceName { get; set; }

		/// <summary>
		/// Optional name to group this definition under in the UI.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Optional left/right order. Lower value = further left.
		/// <para>Defaults to -1, or 0 for classes not decorated with the attribute.</para>
		/// </summary>
		public int Order { get; set; } = -1;

		/// <summary>
		/// Optional notes to display in the UI.
		/// </summary>
		public string Remarks { get; set; }

		///// <summary>
		///// If set to true, attributes are not required on properties and if no attribute is used they will be attempted matched by name.
		///// </summary>
		//public bool AllowAutoMapByName { get; set; }

		/// <summary>
		/// Decorate a class with this for it to be discovered by HC.
		/// <para>At least 1 property must be decorated with <see cref="HCMappedPropertyAttribute"/>.</para>
		/// <para>Required on at least one side of the mapping.</para>
		/// </summary>
		public HCMappedClassAttribute(Type mappedToType)
		{
			MappedToType = mappedToType;
		}
	}
}
