using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.GoTo.Abstractions;
using QoDL.Toolkit.Core.Modules.GoTo.Models;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Modules.GoTo.Services
{
    /// <summary>
    /// Handles comparison of data.
    /// </summary>
    public class TKGoToService : ITKGoToService
    {
        private readonly IEnumerable<ITKGoToResolver> _resolvers;

        /// <summary>
        /// Handles comparison of data.
        /// </summary>
        public TKGoToService(IEnumerable<ITKGoToResolver> typeHandlers)
        {
            _resolvers = typeHandlers;
        }

        /// <inheritdoc />
        public List<TKGoToResolverDefinition> GetResolvers()
        {
            return _resolvers
                .Select(x => new TKGoToResolverDefinition
                {
                    Id = x.GetType().Name,
                    Name = x.Name
                })
                .ToList();
        }

        /// <inheritdoc />
        public async Task<List<TKGoToResolvedDataWithResolverId>> TryLocateInstance(string[] handlerIds, string input)
        {
            var result = new List<TKGoToResolvedDataWithResolverId>();
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
                        result.Add(new TKGoToResolvedDataWithResolverId
                        {
                            ResolverId = resolver.GetType().Name,
                            Data = item
                        });
                    }
                } catch(Exception ex)
                {
                    result.Add(new TKGoToResolvedDataWithResolverId
                    {
                        ResolverId = resolver.GetType().Name,
                        Error = $"Failed resolving data using the resolver implementation '{resolver.GetType().GetFriendlyTypeName()}' with the error:\n{TKExceptionUtils.GetFullExceptionDetails(ex)}"
                    });
                }
            }
            return result;
        }
    }
}
