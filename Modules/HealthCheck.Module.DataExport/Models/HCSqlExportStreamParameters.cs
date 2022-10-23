using HealthCheck.Core.Attributes;
using HealthCheck.Core.Models;

namespace HealthCheck.Module.DataExport.Models
{
    /// <summary></summary>
    public class HCSqlExportStreamParameters
    {
        /// <summary>
        /// What connectionstring to use.
        /// </summary>
        [HCCustomProperty(Name = "ConnectionString", UIHints = HCUIHint.NotNull | HCUIHint.FullWidth)]
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Query to get total result count. Supports placeholder [PREDICATE] that is replaced with <see cref="QueryPredicate"/>.
        /// </summary>
        [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Query to get total result count. Supports the placeholder [PREDICATE].")]
        public string QuerySelectTotalCount { get; set; } = "SELECT COUNT(*)\n[PREDICATE]";

        /// <summary>
        /// Query to get a page of data. Supports placeholder [PREDICATE] that is replaced with <see cref="QueryPredicate"/>.
        /// </summary>
        [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Query to get a single page of data. Available parameters: @pageIndex, @pageSize, @skipCount, @takeCount. Supports the placeholder [PREDICATE].")]
        public string QuerySelectData { get; set; } = "SELECT *\n[PREDICATE]\nORDER BY \nOFFSET @skipCount ROWS\nFETCH NEXT @takeCount ROWS ONLY";

        /// <summary>
        /// Query predicate optionally used in the other selects.
        /// </summary>
        [HCCustomProperty(UIHints = HCUIHint.CodeArea | HCUIHint.NotNull | HCUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Predicate optionally used ")]
        public string QueryPredicate { get; set; } = "FROM ";
    }
}
