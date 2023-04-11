using System;

namespace QoDL.Toolkit.Core.Modules.MappedData.Attributes
{
    /// <summary>
    /// Decorate a class with this for it to be discovered by TK.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class TKMappedClassAttribute : Attribute
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
		/// Optionally provide the name of a method to get <see cref="Mapping"/> from. If provided, <see cref="Mapping"/> is ignored.
		/// <para>Can either be the name of a static method within the decorated class, of on the format ClassName.MethodName where the given class name is a nested class within the decorated class. If using the latter, the method can be non-static.</para>
		/// </summary>
		public string MappingFromMethodName { get; set; }

		/// <summary>
		/// Optionally override display name of this mapping.
		/// </summary>
		public string OverrideName { get; set; }

		/// <summary>
		/// Optional name to group this definition under in the UI.
		/// </summary>
		public string GroupName { get; set; }

		/// <summary>
		/// Optional notes to display in the UI. Supports html.
		/// </summary>
		public string Remarks { get; set; }

		/// <summary>
		/// Html encode any comments in <see cref="Mapping"/>. Set to false to allow html.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool HtmlEncodeMappingComments { get; set; } = true;

		/// <summary>
		/// Decorate a class with this for it to be discovered by TK.
		/// <para>To add comments/remarks use //</para>
		/// <para>Mapping of properties: PropertyName &lt;=&gt; Other or PropertyName &lt;=&gt; [Other1, Other2]</para>
		/// <para>Nested properties: PropertyName {</para>
		/// <para>  NestedProperty &lt;=&gt; etc {</para>
		/// <para>}</para>
		/// </summary>
		public TKMappedClassAttribute(string mapping)
		{
			Mapping = mapping;
		}
	}
}
