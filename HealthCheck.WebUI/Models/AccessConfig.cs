﻿using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

#if NETFULL
using System.Web;
#endif

#if NETCORE
using Microsoft.AspNetCore.Http;
#endif

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Various access options.
    /// </summary>
    public class AccessConfig<TAccessRole>
    {
        /// <summary>
        /// Roles with access to view any stacktrace from failed module loads.
        /// <para>If null nobody can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> ShowFailedModuleLoadStackTrace { get; set; }

        /// <summary>
        /// Roles with access to call the Ping endpoint.
        /// <para>If null anyone can access it by default.</para>
        /// </summary>
        public Maybe<TAccessRole> PingAccess { get; set; }

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

        internal List<ModuleAccessData<TAccessRole>> RoleModuleAccessLevels { get; set; }

        /// <summary>
        /// Grants the given roles access to a module.
        /// <para>ConfigureModuleAccess(MyAccessRoles.Member, ModuleAccess.ViewThing </para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Admin, ModuleAccess.EditThing | ModuleAccess.CreateThing)</para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Guest | MyAccessRoles.Member, ModuleAccess.AnotherThing)</para>
        /// </summary>
        public void GiveRolesAccessToModule<TModuleAccessOptionsEnum>(TAccessRole roles, TModuleAccessOptionsEnum access)
            where TModuleAccessOptionsEnum : Enum
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole> { Roles = roles, AccessOptions = access, AccessOptionsType = typeof(TModuleAccessOptionsEnum) });

        /// <summary>
        /// Grants the given roles access to a module without any specific access options.
        /// </summary>
        public void GiveRolesAccessToModule<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole>
            {
                Roles = roles,
                AccessOptions = null,
                AccessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(typeof(TModule))
            });

        /// <summary>
        /// Grants the given roles access to a module with full access.
        /// </summary>
        public void GiveRolesAccessToModuleWithFullAccess<TModule>(TAccessRole roles)
            where TModule : IHealthCheckModule
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole>
            {
                Roles = roles,
                AccessOptions = null,
                FullAccess = true,
                AccessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(typeof(TModule))
            });
    }
}