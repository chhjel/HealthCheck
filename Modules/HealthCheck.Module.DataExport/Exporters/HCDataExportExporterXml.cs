using HealthCheck.Core.Extensions;
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
	public class HCDataExportExporterXml : HCDataExportExporterStringifiedBase
	{
		/// <inheritdoc />
		public override string DisplayName { get; set; } = "XML";

		/// <inheritdoc />
		public override string FileExtension { get; set; } = ".xml";

		/// <summary>
		/// When true xml output will be prettified.
		/// <para>Defaults to true.</para>
		/// </summary>
		public bool Prettify { get; set; } = true;

		private readonly SerializableObjectDictionaryList _builder = new();

		/// <inheritdoc />
		public override void AppendStringifiedItem(Dictionary<string, object> items, Dictionary<string, string> stringifiedItems, Dictionary<string, string> headers, List<string> headerOrder)
		{
			var itemsRenamed = headerOrder
				.ToDictionaryIgnoreDuplicates(x => headers[x], x => stringifiedItems[x]);

			_builder.ObjectList.Add(itemsRenamed);
		}

		/// <inheritdoc />
		public override byte[] GetContents()
		{
			var serializer = new DataContractSerializer(typeof(SerializableObjectDictionaryList));
            using var memoryStream = new MemoryStream();
            using var xmlWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xmlWriter.Formatting = Prettify ? Formatting.Indented : Formatting.None;
            serializer.WriteObject(xmlWriter, _builder);
            xmlWriter.Flush();
            return memoryStream.ToArray();
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
						writer.WriteElementString(SantizeXmlElementName(kvp.Key), kvp.Value?.ToString());
					}
					writer.WriteEndElement();
				}
			}

			private static string SantizeXmlElementName(string name)
			{
				name = (name ?? "_")
					.Replace(" ", "_")
					.Replace("\t", "_")
					.Trim();
				return XmlConvert.EncodeLocalName(name);
			}
		}
	}
}
