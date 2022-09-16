using HealthCheck.Core.Modules.MappedData.Attributes;
using Newtonsoft.Json;

namespace HealthCheck.Dev.Common.DataMapper
{
	[HCMappedClass(@"
// Name combines first and last name
Name <=> [ExternalData.SomeInfoName1, ExternalData.SomeInfoName2]

HomeAddress {
	FromRootLevelTest <=> ExternalData.RootValue
	StreetName <=> ExternalData.Addresses.StreetName
	StreetNo <=> ExternalData.Addresses.StreetNo // 0 - 12
	ZipCode <=> ExternalData.Addresses.ZipCode
	City <=> ExternalData.Addresses.City
	Geo {
		// Fetched from api X
		Lon
		// Fetched from api X
		Lat
	}
}

WorkAddress {
	FromRootLevelTest <=> ExternalData.RootValue
	StreetName <=> ExternalData.Addresses.Work.StreetName
	StreetNo <=> ExternalData.Addresses.Work.StreetNo
	ZipCode <=> ExternalData.Addresses.Work.ZipCode
	City <=> ExternalData.Addresses.Work.City
	Geo {
		// Fetched from api Y
		Lon
		// Fetched from api Y
		Lat
	}
}
", GroupName = "Test Group X", OverrideName = "Test #1", Remarks = "Some remarks here.")]
	[HCMappedClass(@"
Name <=> ExternalData.SomethingElse
HomeAddress <=> NonExistent.Nope
LeftNotExisting <=> ExternalData.Addresses.Work.City
BothNotExisting <=> Nope
WorkAddress {
	Geo {
		// This one should fail
		Lat <=> ExternalData.Addresses.NotExisting
	}
}
", OverrideName = "With errors")]
	[HCMappedClass(@"Name <=> ExternalData.SomethingElse")]
	public class LeftRoot
	{
		[JsonProperty("info8")]
		public string Name { get; set; }

		public AddressData HomeAddress { get; set; }
		public AddressData WorkAddress { get; set; }
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
	[HCMappedReferencedType]
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
	// ########################################################
}
