using QoDL.Toolkit.Core.Util;
using System;

namespace QoDL.Toolkit.Core.Attributes
{
    /// <summary>
    /// Used to prevent a property from being included in <see cref="TKObjectUtils.TryCreateHtmlSummaryFromProperties"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TKExcludeFromHtmlSummaryAttribute : Attribute { }
}
