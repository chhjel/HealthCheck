using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// If this property is set a login dialog will be shown if the user does not have access to any of the content.
        /// <para>If this property is set, <see cref="RedirectTargetOnNoAccess"/> will have no effect.</para>
        /// <para>If neither this nor <see cref="RedirectTargetOnNoAccess"/> is set, a 404 will be returned.</para>
        /// </summary>
        public HCIntegratedLoginConfig IntegratedLoginConfig { get; set; }
        internal bool UseIntegratedLogin => IntegratedLoginConfig != null;

        /// <summary>
        /// If this property is set a profile dialog button will be available.
        /// </summary>
        public HCIntegratedProfileConfig IntegratedProfileConfig { get; set; }

        /// <summary>
        /// Redirect url if the request does not have access to any of the content.
        /// <para>If neither this nor <see cref="IntegratedLoginConfig"/> is set, a 404 will be returned.</para>
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

        internal void GiveRolesAccessToModule(Type moduleAccessOptionsEnumType, TAccessRole roles, object access, string[] categories, string[] ids = null)
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole> {
                Roles = roles,
                AccessOptions = access,
                AccessOptionsType = moduleAccessOptionsEnumType,
                Categories = categories ?? Array.Empty<string>(),
                Ids = ids ?? Array.Empty<string>()
            });

        /// <summary>
        /// Grants the given roles access to a module.
        /// <para>Optionally limit access to given categories and/or ids.</para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Member, ModuleAccess.ViewThing </para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Admin, ModuleAccess.EditThing | ModuleAccess.CreateThing)</para>
        /// <para>ConfigureModuleAccess(MyAccessRoles.Guest | MyAccessRoles.Member, ModuleAccess.AnotherThing)</para>
        /// </summary>
        public void GiveRolesAccessToModule<TModuleAccessOptionsEnum>(TAccessRole roles, TModuleAccessOptionsEnum access, IEnumerable<string> limitToCategories = null, IEnumerable<string> limitToIds = null)
            where TModuleAccessOptionsEnum : Enum
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole>
            {
                Roles = roles,
                AccessOptions = access,
                AccessOptionsType = typeof(TModuleAccessOptionsEnum),
                Categories = limitToCategories?.ToArray() ?? Array.Empty<string>(),
                Ids = limitToIds?.ToArray() ?? Array.Empty<string>()
            });

        /// <summary>
        /// Grants the given roles access to a module without any specific access options.
        /// <para>Optionally limit access to given categories and/or ids.</para>
        /// </summary>
        public void GiveRolesAccessToModule<TModule>(TAccessRole roles, IEnumerable<string> limitToCategories = null, IEnumerable<string> limitToIds = null)
            where TModule : IHealthCheckModule
            => RoleModuleAccessLevels.Add(new ModuleAccessData<TAccessRole>
            {
                Roles = roles,
                AccessOptions = null,
                AccessOptionsType = HealthCheckModuleLoader.GetModuleAccessOptionsType(typeof(TModule)),
                Categories = limitToCategories?.ToArray() ?? Array.Empty<string>(),
                Ids = limitToIds?.ToArray() ?? Array.Empty<string>()
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
