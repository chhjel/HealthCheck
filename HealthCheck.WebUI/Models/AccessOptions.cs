using HealthCheck.Core.Util;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Various options for the page access.
    /// </summary>
    public class AccessOptions<TAccessRole>
    {
        /// <summary>
        /// Roles with access to view the overview page.
        /// <para>If null anyone can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> OverviewPageAccess { get; set; }

        /// <summary>
        /// Roles with access to view the tests page.
        /// <para>If null anyone can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> TestsPageAccess { get; set; }

        /// <summary>
        /// Roles with access to view the audit logs.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> AuditLogAccess { get; set; }
    }
}
