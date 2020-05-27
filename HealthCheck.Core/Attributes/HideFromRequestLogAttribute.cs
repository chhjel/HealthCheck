using System;

namespace HealthCheck.Core.Attributes
{
    /// <summary>
    /// Any attribute named the same as this one will hide decorated methods from the request log.
    /// <para>Use RequestLogInfoAttribute from the HealthCheck.RequestLog nuget package for more options.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class HideFromRequestLogAttribute : Attribute
    {
    }
}
