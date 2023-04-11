using System.Linq;
using System.Text.RegularExpressions;

namespace QoDL.Toolkit.Core.Util
{
    /// <summary>
    /// Utils related to SQL.
    /// </summary>
    public static class TKSQLUtils
    {
        private static readonly string[] _invalidCommands = new[] { "UPDATE", "INSERT", "DROP", "TRUNCATE", "DELETE", "COMMIT", "GRANT", "CREATE", "REPLACE", "EXEC", "EXECUTE" };
        private static readonly (string, Regex)[] _invalidCommandRegexes;

        static TKSQLUtils()
        {
            _invalidCommandRegexes = _invalidCommands.Select(x => (x, new Regex($@"(^|[^\w]){x}([^\w])", RegexOptions.IgnoreCase))).ToArray();
        }

        /// <summary>
        /// Attempt to find things in the given query that might cause updates in the DB.
        /// <para>Obviously not 100% safe, but can be used for developer use to prevent some e.g. copy-paste mistakes.</para>
        /// </summary>
        public static SQLValidationResult TryCheckQueryForThingsThatCauseChanges(string query)
        {
            static SQLValidationResult createError(string reason)
                => new() { InvalidReason = reason };

            foreach (var regex in _invalidCommandRegexes)
            {
                if (regex.Item2.IsMatch(query)) return createError($"Query seems to contain '{regex.Item1}', not allowed.");
            }

            return new SQLValidationResult { Valid = true };
        }

        /// <summary></summary>
        public class SQLValidationResult
        {
            /// <summary>True if the logic didn't find anything suspicious in the query.</summary>
            public bool Valid { get; set; }
            /// <summary>What, if anything, the logic found suspicious in the query.</summary>
            public string InvalidReason { get; set; }
        }
    }

}
