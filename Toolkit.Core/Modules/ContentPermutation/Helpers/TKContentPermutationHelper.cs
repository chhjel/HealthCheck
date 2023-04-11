using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Attributes;
using QoDL.Toolkit.Core.Modules.ContentPermutation.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.ContentPermutation.Helpers
{
    internal static class TKContentPermutationHelper
    {
        private static readonly object _cacheLock = new();
        private static List<TKContentPermutationType> _cache = null;

        public static List<TKContentPermutationType> GetPermutationTypesCached(IEnumerable<Assembly> assemblies)
        {
            lock (_cacheLock)
            {
                _cache ??= DiscoverPermutationTypes(assemblies);
                return _cache;
            }
        }

        public static List<TKContentPermutationType> DiscoverPermutationTypes(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                return new();
            }

            var classTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<TKContentPermutationTypeAttribute>(inherit: true) != null)
                .ToList();

            var types = classTypes
                .Select(x => CreatePermutationType(x, x.GetCustomAttribute<TKContentPermutationTypeAttribute>(inherit: true)))
                .ToList();

            return types;
        }

        public static TKContentPermutationType CreatePermutationType(Type type, TKContentPermutationTypeAttribute attr)
        {
            var propertyConfigs = TKCustomPropertyAttribute.CreateInputConfigs(type, (c, p, a) =>
            {
                c.Nullable = true;
                c.NullName = "<not filtered>";
                c.DefaultValue = null;
            });

            var perms = TKPermutationUtils.CreatePermutationsOf(type)
                .Select((x, i) => new TKContentPermutationChoice
                {
                    Id = i,
                    Choice = x
                }).ToList();

            return new TKContentPermutationType
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
