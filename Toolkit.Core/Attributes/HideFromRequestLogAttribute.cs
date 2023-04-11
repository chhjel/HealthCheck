using System;

namespace QoDL.Toolkit.Core.Attributes;

/// <summary>
/// Any attribute named the same as this one will hide decorated methods from the request log.
/// <para>Use RequestLogInfoAttribute from the QoDL.Toolkit.RequestLog nuget package for more options.</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
public class HideFromRequestLogAttribute : Attribute
{
}
