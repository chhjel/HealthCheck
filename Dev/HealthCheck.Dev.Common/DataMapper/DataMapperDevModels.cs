using HealthCheck.Core.Modules.MappedData.Attributes;
using Newtonsoft.Json;

namespace HealthCheck.Dev.Common.DataMapper
{
    [HCMappedClass(typeof(Tech_Right), GroupName = "Tech Import", DataSourceName = "TechCRM", Order = -1, OverrideName = "Tech Left", Remarks = "Some remarks here for the type.")]
	public class Tech_Left
	{
		[HCMappedProperty(nameof(Tech_Right.Weight), Remarks = "This one is weird.")]
		//[System.Text.Json.Serialization.JsonPropertyNameAttribute("ASD")]
		[JsonProperty("info2")]
		public string Information2 { get; set; }

		[HCMappedProperty(nameof(Tech_Right.Height))]
		[JsonProperty("info8")]
		public string Information8 { get; set; }

		public int NotRelevantApi { get; set; }

		[HCMappedProperty(nameof(Tech_Right.PriceLocal))]
		public TechPrice_Left Price { get; set; }
	}
	public class TechPrice_Left
	{
		[HCMappedProperty(nameof(TechPrice_Right.AmountNo))]
		public int Amount { get; set; }
		public int NotRelevantPriceA { get; set; }
		[HCMappedProperty(nameof(TechPrice_Right.CurrencyLocal))]
		public TechCurrency_Left Currency { get; set; }
	}
	public class TechCurrency_Left
	{
		[HCMappedProperty(nameof(TechCurrency_Right.CurrencyRight))]
		public string CurrencyLeft { get; set; }
	}

	public class Tech_Right
	{
		public decimal Weight { get; set; }
		[HCMappedProperty(nameof(Tech_Left.Information8), OverrideName = "HAAAIGHT")]
		public decimal Height { get; set; }
		public bool NotRelevantLocal { get; set; }

		public TechPrice_Right PriceLocal { get; set; }
	}
	public class TechPrice_Right
	{
		public decimal AmountNo { get; set; }
		public int NotRelevantPriceB { get; set; }
		public TechCurrency_Right CurrencyLocal { get; set; }
	}
	public class TechCurrency_Right
	{
		public string CurrencyRight { get; set; }
	}
	public class Recursive_Left
	{
		[HCMappedProperty(nameof(Recursive_Right.RecId))]
		public string RecId { get; set; }
		[HCMappedProperty(nameof(Recursive_Right.SubRecursive))]
		public Recursive_Left SubRecursive { get; set; }
	}
	public class Recursive_Right
	{
		public string RecId { get; set; }
		public Recursive_Right SubRecursive { get; set; }
	}

	[HCMappedClass(typeof(string))]
	public class UnusedModel
	{
		[HCMappedProperty(nameof(Tech_Right.Weight), Remarks = "This one is weird.")]
		[JsonProperty("info2")]
		public string Information2 { get; set; }

		public int NotRelevantApi { get; set; }
	}

	[HCMappedClass(typeof(Thing_Right))]
	public class Thing_Left
	{
		[HCMappedProperty(nameof(Thing_Right.SomethingRight))]
		public string SomethingLeft { get; set; }

		[HCMappedProperty(nameof(Thing_Right.CurrRight))]
		public TechCurrency_Left CurrLeft { get; set; }

		[HCMappedProperty(nameof(Thing_Right.RecursiveRight))]
		public Recursive_Left RecursiveLeft { get; set; }
	}

	public class Thing_Right
	{
		public string SomethingRight { get; set; }
		public TechCurrency_Right CurrRight { get; set; }
		public Recursive_Right RecursiveRight { get; set; }
	}
}
