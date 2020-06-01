using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.AccessManager
{
    /// <summary>
    /// Module for managing HC related access.
    /// </summary>
    public class HCAccessManagerModule : HealthCheckModuleBase<HCAccessManagerModule.AccessOption>
    {
        private HCAccessManagerModuleOptions Options { get; }

        /// <summary>
        /// Module for managing HC related access.
        /// </summary>
        public HCAccessManagerModule(HCAccessManagerModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCAccessManagerModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,
        }

        #region Invokable methods
        /// <summary>
        /// Get overview of roles and module options.
        /// </summary>
        [HealthCheckModuleMethod]
        public object GetAccessData(HealthCheckModuleContext context)
        {
            var allRoles = Enum.GetValues(context.CurrentRequestRoles.GetType())
                .OfType<Enum>()
                .Where(x => (int)Convert.ChangeType(x, typeof(int)) > 0)
                .Select(x => new Role()
                {
                    Id = x.ToString(),
                    Name = x.ToString().SpacifySentence()
                });

            var moduleOptions = context.LoadedModules.Select(x => new ModuleAccessData()
                {
                    ModuleName = x.Name,
                    ModuleId = x.Id,
                    AccessOptions = Enum.GetValues(x.AccessOptionsType)
                        .OfType<Enum>()
                        .Where(x => (int)Convert.ChangeType(x, typeof(int)) > 0)
                        .Select(x => new ModuleAccessOption(){
                            Id = x.ToString(),
                            Name = x.ToString().SpacifySentence()
                        }).ToList()
                })
                .ToList();

            return new
            {
                Roles = allRoles,
                ModuleOptions = moduleOptions
            };
        }

        private class ModuleAccessData
        {
            public string ModuleName { get; set; }
            public string ModuleId { get; set; }
            public List<ModuleAccessOption> AccessOptions { get; set; }
        }

        private class ModuleAccessOption
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        private class Role
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        #endregion

        #region Private helpers
        #endregion
    }
}
