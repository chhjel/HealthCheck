﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extension methods related to <see cref="string"/>s.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Appends the given suffix if the string is not null and does not already end with it.
        /// </summary>
        public static string EnsureEndsWithIfNotNull(this string text, string suffix)
        {
            if (text == null) return text;
            else return text.EndsWith(suffix) ? text : $"{text}{suffix}";
        }

        /// <summary>
        /// Joins the given values with the given delimiter. The last two parts will use the given lastDelimiter.
        /// <para>For creating sentence outputs like ["A", "B", "C"] => "A, B and C"</para>
        /// </summary>
        public static string JoinForSentence(this IEnumerable<string> values,
            string delimiter = ", ", string lastDelimiter = " and ", string valueIfNoItems = null)
        {
            if (values == null || !values.Any())
            {
                return valueIfNoItems;
            }
            else if (values.Count() <= 1)
            {
                return values.First();
            }
            else if (values.Count() == 2)
            {
                return string.Join(lastDelimiter, values);
            }

            var firstParts = values.Take(values.Count() - 1);
            var lastPart = values.Last();
            return $"{string.Join(delimiter, firstParts)}{lastDelimiter}{lastPart}";
        }

        /// <summary>
        /// If the input count is not equal to 1 the plural suffix will be appended, or the pluralWord used if given.
        /// </summary>
        public static string Pluralize(this string value, int count,
            string pluralSuffix = "s",
            string pluralWord = null,
            string singularPrefix = null)
            => (count == 1) ? $"{singularPrefix}{value}" : pluralWord ?? $"{value}{pluralSuffix}";

        /// <summary>
        /// Wrap the string with quotes if not null, otherwise return the text null.
        /// </summary>
        public static string QuotifyOrReturnNullText(this string text, string quoteCharacter = "'")
        {
            if (text == null) return "null";
            else return $"{quoteCharacter}{text}{quoteCharacter}";
        }

        /// <summary>
        /// If the string is not null, ensure a dot is at the end.
        /// </summary>
        public static string EnsureDotAtEndIfNotNull(this string text, bool trim = true)
        {
            if (text == null) return null;
            if (trim)
            {
                text = text?.Trim();
            }

            if (!text.EndsWith("."))
            {
                text += ".";
            }

            return text;
        }

        /// <summary>
        /// Capitalizes first character.
        /// </summary>
        public static string CapitalizeFirst(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            else return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        /// <summary>
        /// Appends a space before all capital letters and numbers in a sentence, except the first character.
        /// Also trims and capitalizes first character unless disabled.
        /// </summary>
        public static string SpacifySentence(this string text,
            bool trim = true, bool capitalizeFirst = true)
        {
            if (text == null || text.Length == 0)
            {
                return text;
            }

            var str = new StringBuilder();
            if (trim)
            {
                text = text.Trim();
            }
            if (capitalizeFirst)
            {
                text = text.Substring(0, 1).ToUpper() + text.Substring(1);
            }

            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && (char.IsUpper(text[i]) || ANumberIsStartingAtPosition(text, i)))
                {
                    str.Append(" ");
                }

                str.Append(text[i]);
            }

            return str.ToString();
        }

        private static bool ANumberIsStartingAtPosition(string text, int index)
        {
            if (index > 0 && char.IsDigit(text[index - 1])) return false;
            else return char.IsDigit(text[index]);
        }
    }
}
