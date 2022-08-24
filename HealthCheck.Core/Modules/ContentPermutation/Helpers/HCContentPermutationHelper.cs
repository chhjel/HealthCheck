﻿using HealthCheck.Core.Modules.ContentPermutation.Attributes;
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
        private static List<HCContentPermutationType> _cache = null;

        public static List<HCContentPermutationType> GetPermutationTypesCached(IEnumerable<Assembly> assemblies)
        {
            lock (_cache)
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
            var perms = HCPermutationUtils.CreatePermutationsOf(type)
                .Select((x, i) => new HCContentPermutationChoice
                {
                    Id = i,
                    Choice = x
                }).ToList();
            return new HCContentPermutationType
            {
                Id = type.FullName,
                Type = type,
                Permutations = perms
            };
        }
    }
}
