﻿using HealthCheck.Core.Util;

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

        /// <summary>
        /// Redirect url if the request does not have access to any of the content.
        /// <para>If not set a 404 will be returned.</para>
        /// </summary>
        public string RedirectTargetOnNoAccess { get; set; }
    }
}
