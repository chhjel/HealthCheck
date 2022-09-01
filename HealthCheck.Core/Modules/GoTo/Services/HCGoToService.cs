using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.GoTo.Abstractions;
using HealthCheck.Core.Modules.GoTo.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.GoTo.Services
{
    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public class HCGoToService : IHCGoToService
    {
        private readonly IEnumerable<IHCGoToResolver> _resolvers;

        /// <summary>
        /// Handles comparison of data.
        /// </summary>
        public HCGoToService(IEnumerable<IHCGoToResolver> typeHandlers)
        {
            _resolvers = typeHandlers;
        }

        /// <inheritdoc />
        public List<HCGoToResolverDefinition> GetResolvers()
        {
            return _resolvers
                .Select(x => new HCGoToResolverDefinition
                {
                    Id = x.GetType().Name,
                    Name = x.Name
                })
                .ToList();
        }

        /// <inheritdoc />
        public async Task<List<HCGoToResolvedDataWithResolverId>> TryLocateInstance(string[] handlerIds, string input)
        {
            var result = new List<HCGoToResolvedDataWithResolverId>();
            if (handlerIds?.Any() != true || string.IsNullOrEmpty(input)) return result;

            var resolvers = _resolvers
                .Where(x => handlerIds.Contains(x.GetType().Name))
                .ToArray();
            if (!resolvers.Any()) return result;

            foreach (var resolver in resolvers)
            {
                try
                {
                    var item = await resolver.TryResolveAsync(input);
                    if (item != null)
                    {
                        result.Add(new HCGoToResolvedDataWithResolverId
                        {
                            ResolverId = resolver.GetType().Name,
                            Data = item
                        });
                    }
                } catch(Exception ex)
                {
                    result.Add(new HCGoToResolvedDataWithResolverId
                    {
                        ResolverId = resolver.GetType().Name,
                        Error = $"Failed resolving data using the resolver implementation '{resolver.GetType().GetFriendlyTypeName()}' with the error:\n{HCExceptionUtils.GetFullExceptionDetails(ex)}"
                    });
                }
            }
            return result;
        }
    }
}
