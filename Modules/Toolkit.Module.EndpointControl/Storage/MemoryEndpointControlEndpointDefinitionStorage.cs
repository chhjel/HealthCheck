using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.EndpointControl.Storage
{
    /// <summary>
    /// Stores definition data in memory.
    /// </summary>
    public class MemoryEndpointControlEndpointDefinitionStorage : IEndpointControlEndpointDefinitionStorage
    {
        private readonly List<EndpointControlEndpointDefinition> _data = new();

        /// <inheritdoc />
        public Task ClearAllDefinitions()
        {
            _data.Clear();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task DeleteDefinition(string endpointId)
        {
            _data.RemoveAll(x => x.EndpointId == endpointId);
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool HasDefinitionFor(string endpointId)
        {
            return _data.Any(x => x.EndpointId == endpointId);
        }

        /// <inheritdoc />
        public void StoreDefinition(EndpointControlEndpointDefinition definition)
        {
            if (!_data.Any(x => x.EndpointId == definition.EndpointId))
            {
                _data.Add(definition);
            }
        }

        /// <inheritdoc />
        public IEnumerable<EndpointControlEndpointDefinition> GetDefinitions() => _data;
    }
}
