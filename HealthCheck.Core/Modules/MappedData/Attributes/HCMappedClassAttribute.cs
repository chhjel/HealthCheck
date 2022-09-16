using System;

namespace HealthCheck.Core.Modules.MappedData.Attributes
{
    /// <summary>
    /// Decorate a class with this for it to be discovered by HC.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class HCMappedClassAttribute : Attribute
	{
		/// <summary>
		/// How this class maps to another.
		/// <para>To add comments/remarks use //</para>
		/// <para>Mapping of properties: PropertyName &lt;=&gt; Other or PropertyName &lt;=&gt; [Other1, Other2]</para>
		/// <para>Nested properties: PropertyName {</para>
		/// <para>  NestedProperty &lt;=&gt; etc {</para>
		/// <para>}</para>
		/// </summary>
		public string Mapping { get; set; }

		/// <summary>
		/// Optionally override display name of this mapping.
		/// </summary>
		public string OverrideName { get; set; }

		/// <summary>
		/// Optional name to group this definition under in the UI.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Optional notes to display in the UI.
		/// </summary>
		public string Remarks { get; set; }

		/// <summary>
		/// Decorate a class with this for it to be discovered by HC.
		/// <para>To add comments/remarks use //</para>
		/// <para>Mapping of properties: PropertyName &lt;=&gt; Other or PropertyName &lt;=&gt; [Other1, Other2]</para>
		/// <para>Nested properties: PropertyName {</para>
		/// <para>  NestedProperty &lt;=&gt; etc {</para>
		/// <para>}</para>
		/// </summary>
		public HCMappedClassAttribute(string mapping)
		{
			Mapping = mapping;
		}
	}
}
