using System;
using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Util;

/// <summary>
/// Enum related utility methods.
/// </summary>
public static class EnumUtils
{
    /// <summary>
    /// Attempts to get the name of all the flagged enum values. Should never fail and returns an empty list if anything goes wrong.
    /// </summary>
    public static List<string> TryGetEnumFlaggedValueNames(object enm)
    {
        try
        {
            if (enm == null) return new List<string>();
            return GetFlaggedEnumValues(enm)
                ?.Select(x => x?.ToString())
                ?.Where(x => x != null)
                ?.ToList();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// Check if the given object is an enum object with flags attribute.
    /// </summary>
    public static bool IsEnumFlag(object obj)
        => obj is Enum && obj.GetType().GetCustomAttributes(false).Any(x => x.GetType() == typeof(FlagsAttribute));

    /// <summary>
    /// Check if the given type is an enum with flags attribute.
    /// </summary>
    public static bool IsTypeEnumFlag(Type type)
        => type.IsEnum && type.GetCustomAttributes(false).Any(x => x.GetType() == typeof(FlagsAttribute));

    /// <summary>
    /// Check if the given object is an enum object with flags attribute, and the underlying type is one of the given types.
    /// </summary>
    public static bool IsEnumFlagOfType(object obj, Type[] allowedUnderlyingTypes)
    {
        if (!IsEnumFlag(obj))
            return false;

        var underlyingType = Enum.GetUnderlyingType(obj.GetType());
        if (allowedUnderlyingTypes == null || allowedUnderlyingTypes.Length == 0 || !allowedUnderlyingTypes.Contains(underlyingType))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Check if the given enum flag object has the given flags set.
    /// </summary>
    public static bool IsFlagSet(object obj, object flagToCheckFor, bool zeroReturnsFalse = true)
    {
        var underlyingType = Enum.GetUnderlyingType(obj.GetType());
        if (underlyingType == typeof(int))
        {
            return !((int)flagToCheckFor == 0 && zeroReturnsFalse) && ((int)obj & (int)flagToCheckFor) == (int)flagToCheckFor;
        }
        else if (underlyingType == typeof(byte))
        {
            return !((byte)flagToCheckFor == 0 && zeroReturnsFalse) && ((byte)obj & (byte)flagToCheckFor) == (byte)flagToCheckFor;
        }
        else
        {
            throw new NotImplementedException($"Support for enums of the type '{underlyingType.Name}' is not implemented yet. Only int or byte for now.");
        }
    }

    /// <summary>
    /// Get all flagged values.
    /// </summary>
    public static List<object> GetFlaggedEnumValues(object obj)
    {
        var results = new List<object>();
        if (!IsEnumFlag(obj))
            return results;
        var type = obj.GetType();

        foreach (var flagToCheckFor in Enum.GetValues(type))
        {
            var isFlagged = IsFlagSet(obj, flagToCheckFor);
            if (isFlagged)
            {
                results.Add(flagToCheckFor);
            }
        }
        return results;
    }

    /// <summary>
    /// Get all flagged values.
    /// </summary>
    public static List<T> GetFlaggedEnumValues<T>(object obj)
        where T : Enum
    {
        var results = new List<T>();
        if (!IsEnumFlag(obj))
            return results;
        var type = obj.GetType();

        foreach (var flagToCheckFor in Enum.GetValues(type))
        {
            var isFlagged = IsFlagSet(obj, flagToCheckFor);
            if (isFlagged)
            {
                results.Add((T)flagToCheckFor);
            }
        }
        return results;
    }

    /// <summary>
    /// Check if the given object has any of the given flags set.
    /// </summary>
    public static bool EnumFlagHasAnyFlagsSet<TEnum>(TEnum enm, TEnum flagsToCheckFor)
        => EnumFlagHasAnyFlagsSet((object)enm, flagsToCheckFor);

    /// <summary>
    /// Check if the given object has any of the given flags set.
    /// </summary>
    public static bool EnumFlagHasAnyFlagsSet(object obj, object flagsToCheckFor)
    {
        var flaggedValues = GetFlaggedEnumValues(obj);
        var toCheckFor = GetFlaggedEnumValues(flagsToCheckFor);
        return toCheckFor.Any(x => flaggedValues.Contains(x));
    }

    /// <summary>
    /// Check if the given object has all of the given flags set.
    /// </summary>
    public static bool EnumFlagHasAllFlagsSet(object obj, object flagsToCheckFor)
    {
        var flaggedValues = GetFlaggedEnumValues(obj);
        var toCheckFor = GetFlaggedEnumValues(flagsToCheckFor);
        return toCheckFor.All(x => flaggedValues.Contains(x));
    }
}
