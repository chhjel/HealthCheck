using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.ContentPermutation.Attributes;
using HealthCheck.Core.Modules.ContentPermutation.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.ContentPermutation.Helpers
{
    internal static class HCContentPermutationHelper
    {
        private static readonly object _cacheLock = new();
        private static List<HCContentPermutationType> _cache = null;

        public static List<HCContentPermutationType> GetPermutationTypesCached(IEnumerable<Assembly> assemblies)
        {
            lock (_cacheLock)
            {
                if (_cache == null)
                {
                    _cache = DiscoverPermutationTypes(assemblies);
                }
                return _cache;
            }
        }

        public static List<HCContentPermutationType> DiscoverPermutationTypes(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                return new();
            }

            var classTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<HCContentPermutationTypeAttribute>(inherit: true) != null)
                .ToList();

            var types = classTypes
                .Select(x => CreatePermutationType(x, x.GetCustomAttribute<HCContentPermutationTypeAttribute>(inherit: true)))
                .ToList();

            return types;
        }

        public static HCContentPermutationType CreatePermutationType(Type type, HCContentPermutationTypeAttribute attr)
        {
            var propertyConfigs = HCCustomPropertyAttribute.CreateInputConfigs(type, (c, p, a) =>
            {
                c.Nullable = true;
                c.NullName = "<not filtered>";
                c.DefaultValue = null;
            });

            var perms = HCPermutationUtils.CreatePermutationsOf(type)
                .Select((x, i) => new HCContentPermutationChoice
                {
                    Id = i,
                    Choice = x
                }).ToList();

            return new HCContentPermutationType
            {
                Id = type.FullName,
                Name = attr.Name ?? type.Name.SpacifySentence(),
                Description = attr.Description,
                MaxAllowedContentCount = attr.MaxAllowedContentCount,
                DefaultContentCount = attr.DefaultContentCount,
                Type = type,
                Permutations = perms,
                PropertyConfigs = propertyConfigs
            };
        }
    }
}
