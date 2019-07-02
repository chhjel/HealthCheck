using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Util
{
    internal static class EnumUtils
    {
        public static bool IsEnumFlag(object obj)
            => obj is Enum && obj.GetType().GetCustomAttributes(false).Any(x => x.GetType() == typeof(FlagsAttribute));

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

        public static bool IsFlagSet(object obj, object flagToCheckFor, bool zeroReturnsFalse = true)
        {
            var underlyingType = Enum.GetUnderlyingType(obj.GetType());
            if (underlyingType == typeof(int))
            {
                return ((int)flagToCheckFor == 0 && zeroReturnsFalse) ? false : ((int)obj & (int)flagToCheckFor) == (int)flagToCheckFor;
            }
            else if (underlyingType == typeof(byte))
            {
                return ((byte)flagToCheckFor == 0 && zeroReturnsFalse) ? false : ((byte)obj & (byte)flagToCheckFor) == (byte)flagToCheckFor;
            }
            else
            {
                throw new NotImplementedException($"Support for enums of the type '{underlyingType.Name}' is not implemented yet. Only int or byte for now.");
            }
        }

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

        public static bool EnumFlagHasAnyFlagsSet(object obj, object flagsToCheckFor)
        {
            var flaggedValues = GetFlaggedEnumValues(obj);
            var toCheckFor = GetFlaggedEnumValues(flagsToCheckFor);
            return toCheckFor.Any(x => flaggedValues.Contains(x));
        }
    }
}
