using HealthCheck.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HealthCheck.Module.DataExport.Exporters
{
    /// <summary>
    /// Outputs xml files.
    /// </summary>
    public class HCDataExportExporterXml : IHCDataExportExporter
	{
		/// <inheritdoc />
		public string DisplayName { get; set; } = "XML";

		/// <inheritdoc />
		public string FileExtension { get; set; } = ".xml";

		/// <summary>
		/// When true xml output will be prettified.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool Prettify { get; set; } = true;

		private readonly SerializableObjectDictionaryList _builder = new();

		/// <inheritdoc />
		public void SetHeaders(List<string> headers) { }

		/// <inheritdoc />
		public void AppendItem(Dictionary<string, object> items, Dictionary<string, string> itemsStringified, List<string> order)
			=> _builder.ObjectList.Add(itemsStringified);

		/// <inheritdoc />
		public byte[] GetContents()
		{
			var serializer = new DataContractSerializer(typeof(SerializableObjectDictionaryList));
			using (var memoryStream = new MemoryStream())
			{
				using (var xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
				{
					xmlWriter.Formatting = Prettify ? System.Xml.Formatting.Indented : System.Xml.Formatting.None;
					serializer.WriteObject(xmlWriter, _builder);
					xmlWriter.Flush();
					return memoryStream.ToArray();
				}
			}
		}

		[Serializable]
		[XmlRoot(ElementName = "ExportedData")]
		private class SerializableObjectDictionaryList : IXmlSerializable
		{
			public List<Dictionary<string, string>> ObjectList { get; set; } = new();

			public XmlSchema GetSchema() => new();

			public void ReadXml(XmlReader reader) { }

			public void WriteXml(XmlWriter writer)
			{
				foreach (var dict in ObjectList)
				{
					writer.WriteStartElement("Item");
					foreach (var kvp in dict)
					{
						writer.WriteElementString(kvp.Key, kvp.Value?.ToString());
					}
					writer.WriteEndElement();
				}
			}
		}
	}
}
