using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Tests;
using HealthCheck.Core.Util.Modules;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace HealthCheck.DevTest._TestImplementation.Modules
{
    public class HCTestsModuleExt : HCTestsModule
    {
        public HCTestsModuleExt(HCTestsModuleOptions options) : base(options)
        {
        }

        private static readonly Regex _downloadFileUrlRegex
            = new(@"^/TMDownloadFile/(?<type>[\w-]+)?__(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);
        public override object TMDownloadFile(HealthCheckModuleContext context, string url)
        {
            var match = _downloadFileUrlRegex.Match(url);
            if (!match.Success)
            {
                return null;
            }

            // Parse url
            var idFromUrl = match.Groups["id"].Value.Trim();
            var typeUrlMatch = match.Groups["type"];
            string typeFromUrl = typeUrlMatch.Success ? typeUrlMatch.Value : null;

            if (!IsFileDownloadAllowedForSession(typeFromUrl, idFromUrl))
            {
                return null;
            }
            else if (Options.FileDownloadHandler == null)
            {
                return HealthCheckFileDownloadResult.CreateFromString("not_configured.txt",
                    $"FileDownloadHandler has not been configured. Please set {nameof(HCTestsModuleOptions)}.{nameof(HCTestsModuleOptions.FileDownloadHandler)}.");
            }

            var file = Options.FileDownloadHandler?.Invoke(typeFromUrl, idFromUrl);
            if (file == null)
            {
                return null;
            }

            // Store audit data
            context.AddAuditEvent("File download", file.FileName)
                .AddClientConnectionDetails(context)
                .AddDetail("File Name", file.FileName);

            return file;
        }

        protected override bool IsFileDownloadAllowedForSession(string type, string id)
        {
            lock (AllowedDownloadsCache)
            {
                try
                {
                    var sessionId = HCGlobalConfig.GetCurrentSessionId?.Invoke();
                    if (string.IsNullOrWhiteSpace(sessionId))
                    {
                        return false;
                    }

                    var key = $"{sessionId}__{type}__{id}";
                    var list = AllowedDownloadsCache[key];
                    return list?.Any(x => x[0] == type && x[1] == id) == true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}