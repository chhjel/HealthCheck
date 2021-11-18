using HealthCheck.Core.Modules.Tests.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace HealthCheck.Core.Modules.Tests.Utils
{
    internal static class AutoResultHelper
    {
        // Relative or absolute url.
        private static readonly Regex _urlRegex
            = new(@"(https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}|\/)\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        private static readonly Regex _absoluteUrlRegex
            = new(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)");
        private static readonly string[] _imgExtensions = new[] 
            { "APNG", "AVIF", "GIF", "JPG", "JPEG", "PNG", "SVG", "WEBP", "BMP", "ICO", "TIFF" };
        private static readonly Type[] _stringifiableTypes = new[]
            { typeof(string), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid) };

        internal static void DefaultAutoResultAction(TestResult result, object obj)
        {
            if (obj == null)
            {
                return;
            }

            var type = obj.GetType();
            if (type.IsPrimitive || _stringifiableTypes.Contains(type))
            {
                ProcessStringifiableResult(result, obj);
            }

            if (obj is string str)
            {
                ProcessStringResult(result, str);
            }

            if (obj is Exception ex)
            {
                result.AddExceptionData(ex);
            }
        }

        private static void ProcessStringifiableResult(TestResult result, object obj)
        {
            if (result.AllowOverrideMessage)
            {
                var str = obj.ToString();
                if (str.Length < 100)
                {
                    result.Message += $" Result was \"{str}\".";
                }
            }
        }

        private static void ProcessStringResult(TestResult result, string str)
        {
            var absoluteUrls = _absoluteUrlRegex.Matches(str).OfType<Match>().Select(x => x.Value).ToArray();
            var relativeUrlMatch = _urlRegex.Match(str);

            if (absoluteUrls.Any())
            {
                foreach(var url in absoluteUrls)
                {
                    AddUrl(result, url);
                }
            }
            else if (relativeUrlMatch.Success)
            {
                AddUrl(result, relativeUrlMatch.Value);
            }
        }

        private static void AddUrl(TestResult result, string str)
        {
            result.AddUrlData(str);

            var isImage = _imgExtensions.Any(ext => str.ToLower().EndsWith($".{ext.ToLower()}"));
            if (isImage)
            {
                result.AddImageUrlData(str);
            }
        }
    }
}
