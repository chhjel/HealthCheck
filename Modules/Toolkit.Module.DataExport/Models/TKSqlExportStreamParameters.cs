using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Models;

namespace QoDL.Toolkit.Module.DataExport.Models
{
    /// <summary></summary>
    public class TKSqlExportStreamParameters
    {
        /// <summary>
        /// What connectionstring to use.
        /// </summary>
        [TKCustomProperty(Name = "ConnectionString", UIHints = TKUIHint.NotNull | TKUIHint.FullWidth)]
        public string ConnectionStringName { get; set; }

        /// <summary>
        /// Query to get total result count. Supports placeholder [PREDICATE] that is replaced with <see cref="QueryPredicate"/>.
        /// </summary>
        [TKCustomProperty(UIHints = TKUIHint.CodeArea | TKUIHint.NotNull | TKUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Query to get total result count. Supports the placeholder [PREDICATE].")]
        public string QuerySelectTotalCount { get; set; } = "SELECT COUNT(*)\n[PREDICATE]";

        /// <summary>
        /// Query to get a page of data. Supports placeholder [PREDICATE] that is replaced with <see cref="QueryPredicate"/>.
        /// </summary>
        [TKCustomProperty(UIHints = TKUIHint.CodeArea | TKUIHint.NotNull | TKUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Query to get a single page of data. Available parameters: @pageIndex, @pageSize, @skipCount, @takeCount. Supports the placeholder [PREDICATE].")]
        public string QuerySelectData { get; set; } = "SELECT *\n[PREDICATE]\nORDER BY \nOFFSET @skipCount ROWS\nFETCH NEXT @takeCount ROWS ONLY";

        /// <summary>
        /// Query predicate optionally used in the other selects.
        /// </summary>
        [TKCustomProperty(UIHints = TKUIHint.CodeArea | TKUIHint.NotNull | TKUIHint.FullWidth, CodeLanguage = "sql",
            Description = "Value here is inserted in the other queries using the placeholders [PREDICATE] to prevent having to write the same predicate twice.")]
        public string QueryPredicate { get; set; } = "FROM ";
    }
}
