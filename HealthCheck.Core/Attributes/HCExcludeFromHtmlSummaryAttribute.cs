using HealthCheck.Core.Util;
using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Used to prevent a property from being included in <see cref="HCObjectUtils.TryCreateHtmlSummaryFromProperties"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HCExcludeFromHtmlSummaryAttribute : Attribute { }
}
