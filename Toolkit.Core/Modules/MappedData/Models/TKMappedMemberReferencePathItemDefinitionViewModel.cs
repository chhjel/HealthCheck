namespace QoDL.Toolkit.Core.Modules.MappedData.Models;

/// <summary></summary>
public class TKMappedMemberReferencePathItemDefinitionViewModel
{
    /// <summary></summary>
    public bool Success { get; set; }
    /// <summary></summary>
    public string Error { get; set; }
    /// <summary></summary>
    public string PropertyName { get; set; }
    /// <summary></summary>
    public string DisplayName { get; set; }
    /// <summary></summary>
    public string DeclaringTypeName { get; set; }
    /// <summary></summary>
    public string PropertyTypeName { get; set; }
    /// <summary></summary>
    public string FullPropertyTypeName { get; set; }
    /// <summary></summary>
    public bool IsHardCoded => HardCodedValue != null;
    /// <summary></summary>
    public string HardCodedValue { get; set; }
}
