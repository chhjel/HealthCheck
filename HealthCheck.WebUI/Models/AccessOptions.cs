﻿using HealthCheck.Core.Entities;
using HealthCheck.Core.Util;
using System;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

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
        /// Roles with access to view the log viewer page.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> LogViewerPageAccess { get; set; }

        /// <summary>
        /// Roles with access to view the requestlog page.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> RequestLogPageAccess { get; set; }

        /// <summary>
        /// Roles with access to clear the requestlog.
        /// <para>If null nobody can clear it by default.</para>
        /// <para>The role must also have RequestLogPageAccess in order to see the clear-button.</para>
        /// </summary>
        public Maybe<TAccessRole> ClearRequestLogAccess { get; set; }

        /// <summary>
        /// Roles with access to view the audit logs.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> AuditLogAccess { get; set; }

        /// <summary>
        /// Roles with access to view the invalid tests.
        /// <para>If null anyone can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> InvalidTestsAccess { get; set; }

        /// <summary>
        /// Roles with access to view developer details on <see cref="SiteEvent"/>.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> SiteEventDeveloperDetailsAccess { get; set; }

        /// <summary>
        /// Roles with access to call the Ping endpoint.
        /// <para>If null anyone can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> PingAccess { get; set; }

        /// <summary>
        /// Roles with access to view the documentation page.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> DocumentationPageAccess { get; set; }

        /// <summary>
        /// Roles with access to view the dataflow page.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> DataflowPageAccess { get; set; }

        /// <summary>
        /// Redirect url if the request does not have access to any of the content.
        /// <para>If not set a 404 will be returned.</para>
        /// </summary>
        public string RedirectTargetOnNoAccess { get; set; }

#if NETFULL
        /// <summary>
        /// Redirect url if the request does not have access to any of the content.
        /// <para>If not set a 404 will be returned.</para>
        /// <para>Takes priority over <see cref="RedirectTargetOnNoAccess"/>.</para>
        /// </summary>
        public Func<HttpRequestBase, string> RedirectTargetOnNoAccessUsingRequest { get; set; }
#endif

#if NETCORE
        /// <summary>
        /// Redirect url if the request does not have access to any of the content.
        /// <para>If not set a 404 will be returned.</para>
        /// <para>Takes priority over <see cref="RedirectTargetOnNoAccess"/>.</para>
        /// </summary>
        public Func<HttpRequest, string> RedirectTargetOnNoAccessUsingRequest { get; set; }
#endif
    }
}
