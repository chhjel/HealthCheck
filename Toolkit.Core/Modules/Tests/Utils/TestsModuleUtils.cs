using QoDL.Toolkit.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils;

internal static class TestsModuleUtils
{
    public static bool IsBuiltInSupportedType(Type type)
    {
        if (_builtInTypeSupport.Contains(type)
            || _builtInTypeNameSupport.Contains(type.Name)
            || type.IsEnum)
        {
            return true;
        }

        // Nullable enum type
        if (type.IsNullable() && type.GenericTypeArguments[0].IsEnum)
        {
            return true;
        }

        // List<T> with supported type
        if (type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var innerType = type.GetGenericArguments()[0];
            if (_builtInTypeSupport.Contains(innerType)
                || _builtInTypeNameSupport.Contains(innerType.Name))
            {
                return true;
            }
        }

        return false;
    }

    private static readonly string[] _builtInTypeNameSupport = new[]
    {
        "HttpPostedFileBase"
    };

    private static readonly Type[] _builtInTypeSupport = new[]
    {
        typeof(string),
        typeof(int), typeof(int?),
        typeof(long), typeof(long?),
        typeof(float), typeof(float?),
        typeof(double), typeof(double?),
        typeof(decimal), typeof(decimal?),
        typeof(bool), typeof(bool?),
        typeof(DateTime), typeof(DateTime?),
        typeof(DateTimeOffset), typeof(DateTimeOffset?),
        typeof(DateTime[]), typeof(DateTime?[]),
        typeof(DateTimeOffset[]), typeof(DateTimeOffset?[]),
        typeof(Enum),
        typeof(CancellationToken),
        typeof(Guid), typeof(Guid?)
    };
}
