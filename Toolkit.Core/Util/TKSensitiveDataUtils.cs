using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Utility for stripping sensitive data.
    /// </summary>
    public class TKSensitiveDataUtils
    {
        private static readonly TKCachedRegexContainer _regexCache = new();

        /// <summary>
        /// Masks all numbers with length 11.
        /// </summary>
        public static string MaskNorwegianNINs(string input)
            => MaskAllNumbersWithLength(input, 11);

        /// <summary>
        /// Masks all numbers with the given length.
        /// </summary>
        /// <param name="input">Text to strip values from.</param>
        /// <param name="length">Lenght of numbers to strip.</param>
        /// <param name="mask">Character to replace digits with, set to null to remove them.</param>
        public static string MaskAllNumbersWithLength(string input, int length, char? mask = '*')
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var pattern = $"(?<=^|[^0-9])[0-9]{{{length}}}(?=[^0-9]|$)";
            var regex = _regexCache.GetRegex(pattern, false);
            var replacement = mask != null ? new string(mask.Value, length) : string.Empty;
            return regex.Replace(input, replacement);
        }

        /// <summary>
        /// Masks all email addresses in the given input.
        /// </summary>
        /// <param name="input">Text to strip values from.</param>
        /// <param name="mask">Character to replace use as a mask, set to null to remove text instead.</param>
        /// <param name="nameMaskLevel">How much to mask the name-part.</param>
        /// <param name="domainMaskLevel">How much to mask the domain-part.</param>
        /// <param name="topLevelDomainMaskLevel">How much to mask the top level domain-part.</param>
        /// <returns></returns>
        public static string MaskAllEmails(string input, char? mask = '*',
            EmailMaskLevel nameMaskLevel = EmailMaskLevel.Partial,
            EmailMaskLevel domainMaskLevel = EmailMaskLevel.Partial,
            EmailMaskLevel topLevelDomainMaskLevel = EmailMaskLevel.None)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            string maskValue(string value, EmailMaskLevel level)
            {
                if (level == EmailMaskLevel.None || string.IsNullOrWhiteSpace(value)) return value;
                else if (level == EmailMaskLevel.Full || value.Length < 4) return mask != null ? new string(mask.Value, value.Length) : string.Empty;

                var lengthToKeep = 2;
                var maskLength = value.Length - lengthToKeep;
                var resultingMask = mask != null ? new string(mask.Value, maskLength) : string.Empty;
                return $"{resultingMask}{value.Substring(maskLength)}";
            }

            var matches = _mailRegex.Matches(input).OfType<Match>();
            foreach (var match in matches)
            {
                var name = match.Groups["name"].Value;
                var domain = match.Groups["domain"].Value;
                var tld = match.Groups["tld"].Value;
                var replacement = $"{maskValue(name, nameMaskLevel)}@{maskValue(domain, domainMaskLevel)}.{maskValue(tld, topLevelDomainMaskLevel)}";
                input = input.Replace(match.Value, replacement);
            }
            return input;
        }
        private static readonly Regex _mailRegex = new(@"(?<name>[^\s<>]+)@(?<domain>[^\s<>]+)\.(?<tld>[^\s<>]+)");

        /// <summary>
        /// How much to mask a mail when using <see cref="MaskAllEmails"/>
        /// </summary>
        public enum EmailMaskLevel
        {
            /// <summary>
            /// Mask the whole value.
            /// </summary>
            Full,

            /// <summary>
            /// Mask all but the last 2 characters, or the whole part if the value is less than 4 characters long.
            /// </summary>
            Partial,

            /// <summary>
            /// Don't mask anything.
            /// </summary>
            None
        }
    }
}
