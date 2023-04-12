using System;

namespace QoDL.Toolkit.Core.Models;

/// <summary></summary>
public class TKTypeNamePair
{
    /// <summary></summary>
    public Type Type { get; set; }
    /// <summary></summary>
    public string Name { get; set; }
    /// <summary></summary>
    public TKTypeNameNamePair ToNamed() => new() { Name = Name, TypeName = Type?.Name };
}
