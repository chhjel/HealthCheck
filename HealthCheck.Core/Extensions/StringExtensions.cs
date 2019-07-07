using System.Text;

namespace HealthCheck.Core.Extensions
{
    /// <summary>
    /// Extension methods related to <see cref="string"/>s.
    /// </summary>
    public static class StringExtensions
    {
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
