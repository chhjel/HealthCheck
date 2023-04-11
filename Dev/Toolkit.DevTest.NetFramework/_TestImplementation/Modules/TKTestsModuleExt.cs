using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.Tests;
using QoDL.Toolkit.Core.Util.Modules;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.DevTest._TestImplementation.Modules
{
    public class TKTestsModuleExt : TKTestsModule
    {
        public TKTestsModuleExt(TKTestsModuleOptions options) : base(options)
        {
        }

        private static readonly Regex _downloadFileUrlRegex
            = new(@"^/TMDownloadFile/(?<type>[\w-]+)?__(?<id>[\w-]+)/?", RegexOptions.IgnoreCase);
        public override object TMDownloadFile(ToolkitModuleContext context, string url)
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
                return ToolkitFileDownloadResult.CreateFromString("not_configured.txt",
                    $"FileDownloadHandler has not been configured. Please set {nameof(TKTestsModuleOptions)}.{nameof(TKTestsModuleOptions.FileDownloadHandler)}.");
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
                    var sessionId = TKGlobalConfig.GetCurrentSessionId?.Invoke();
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