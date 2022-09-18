using HealthCheck.Core.Modules.MappedData.Attributes;
using Newtonsoft.Json;

namespace HealthCheck.Dev.Common.DataMapper
{
	[HCMappedClass("", GroupName = "Test Group X", OverrideName = "Test #1", Remarks = "Some remarks here, maybe with <a href=\"#\">link</a>.", MappingFromMethodName = "Mapping.GetMapping", HtmlEncodeMappingComments = false)]
	[HCMappedClass(@"
Name <=> ExternalData.SomethingElse
HomeAddress <=> NonExistent.Nope
LeftNotExisting <=> ExternalData.Addresses.Work.City
BothNotExisting <=> Nope
// This one does not use <b>html</b>.
WorkAddress {
	Geo {
		// This one should fail
		Lat <=> ExternalData.Addresses.NotExisting
	}
}
", OverrideName = "With errors")]
	[HCMappedClass(@"Value <=> ExternalData2.SuperValue")]
	public class LeftRoot
	{
		[JsonProperty("info8")]
		public string Name { get; set; }

		[JsonProperty("ageValue")]
		public int Age { get; set; }

		public decimal Value { get; set; }

		public AddressData HomeAddress { get; set; }
		public AddressData WorkAddress { get; set; }

		public class Mapping
		{
			public string GetMapping()
				=> @"
// Name combines first and last name
Name <=> [ExternalData.SomeInfoName1, AnotherMappedToType.MiddleName, ExternalData.SomeInfoName2]
Age <=> AnotherMappedToType.Age

HomeAddress {
	FromRootLevelTest <=> ExternalData.RootValue
	StreetName <=> ExternalData.Addresses.StreetName
	StreetNo <=> ExternalData.Addresses.StreetNo // 0 - 12
	ZipCode <=> ExternalData.Addresses.ZipCode
	City <=> ExternalData.Addresses.City
	Geo {
		// Fetched from api X1
		Lon
		// Fetched from api X2
		Lat
	}
}

// This <b>one</b> has some <a href=""/test"">html</a>. //
WorkAddress {
	FromRootLevelTest <=> ExternalData.RootValue
	StreetName <=> ExternalData.Addresses.Work.StreetName
	StreetNo <=> ExternalData.Addresses.Work.StreetNo
	ZipCode <=> ExternalData.Addresses.Work.ZipCode
	City <=> ExternalData.Addresses.Work.City
	Geo {
		// Fetched from api Y3
		Lon
		// Fetched from api Y4
		Lat
	}
}
";
		}
	}
	public class AddressData
	{
		public string FromRootLevelTest { get; set; }

		public string StreetName { get; set; }
		public string StreetNo { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
		public GeoData Geo { get; set; }
	}
	public class GeoData
	{
		public decimal Lon { get; set; }
		public decimal Lat { get; set; }
	}
	// -------------------
	[HCMappedReferencedType(DisplayName = "ExtData Display Name", NameInMapping = "Ext", Remarks = "This one is a test with some comments here.")]
	public class ExternalData
	{
		public string RootValue { get; set; }
		public string SomeInfoName1 { get; set; }
		public string SomeInfoName2 { get; set; }
		public ExternalAddressData Addresses { get; set; }
	}
	public class ExternalAddressData
	{
		public string StreetName { get; set; }
		public string StreetNo { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
		public ExternalAddressDetailsData Work { get; set; }
	}
	public class ExternalAddressDetailsData
	{
		public string StreetName { get; set; }
		public string StreetNo { get; set; }
		public string ZipCode { get; set; }
		public string City { get; set; }
	}
	// -------------------
	[HCMappedReferencedType]
	public class AnotherMappedToType
	{
		public string MiddleName { get; set; }
		public int Age { get; set; }
	}
	// -------------------
	[HCMappedReferencedType]
	public class ExternalData2
	{
		public string SuperValue { get; set; }
	}
	// ########################################################
}
