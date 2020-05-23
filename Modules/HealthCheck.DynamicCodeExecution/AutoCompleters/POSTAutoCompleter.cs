using HealthCheck.DynamicCodeExecution.Abstractions;
using HealthCheck.DynamicCodeExecution.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HealthCheck.DynamicCodeExecution.AutoCompleters
{
    /// <summary>
    /// Auto-complete by delegating the job somewhere else through a POST request.
    /// </summary>
    public class POSTAutoCompleter : IDynamicCodeAutoCompleter
    {
        /// <summary>
        /// Target endpoint url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Deserialize the response body to a list of completion datas.
        /// </summary>
        public Func<string, IEnumerable<IDynamicCodeCompletionData>> Deserializer { get; set; }

        /// <summary>
        /// Serialize the request into a string that will be sent to the endpoint.
        /// </summary>
        public Func<CompletionRequest, string> Serializer { get; set; }

        /// <summary>
        /// Optional config for the web client.
        /// </summary>
        public Action<WebClient> WebClientConfig { get; set; }

        /// <summary></summary>
        public async Task<IEnumerable<IDynamicCodeCompletionData>> GetAutoCompleteSuggestionsAsync(string code, string[] assemblyLocations, int position)
        {
            if (Deserializer == null || Serializer == null)
                return await Task.FromResult<IEnumerable<IDynamicCodeCompletionData>>(Enumerable.Empty<AutoCompleteData>());

            var request = new CompletionRequest()
            {
                Code = code,
                AssemblyLocations = assemblyLocations,
                Position = position
            };

            var data = Serializer(request);

            using (var client = new WebClient())
            {
                WebClientConfig?.Invoke(client);
                var response = await client.UploadStringTaskAsync(Url, data);
                return Deserializer(response);
            }
        }
    }
}
