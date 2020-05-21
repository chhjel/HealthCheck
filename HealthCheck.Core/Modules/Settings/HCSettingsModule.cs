using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.Settings.Models;
using System;
using System.Linq;

namespace HealthCheck.Core.Modules.Settings
{
    /// <summary>
    /// Module for configuring custom settings.
    /// </summary>
    public class HCSettingsModule : HealthCheckModuleBase<HCSettingsModule.AccessOption>
    {
        private HCSettingsModuleOptions Options { get; }

        /// <summary>
        /// Module for configuring custom settings.
        /// </summary>
        public HCSettingsModule(HCSettingsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(AccessOption access) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(AccessOption access) => new HCSettingsModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            Nothing = 0,

            /// <summary>Allowed to change settings.</summary>
            ChangeSettings = 1
        }

        #region Invokable methods
        /// <summary>
        /// Get settings.
        /// </summary>
        [HealthCheckModuleMethod]
        public GetSettingsViewModel GetSettings()
        {
            var settings = Options.SettingsService.GetSettingItems();
            return new GetSettingsViewModel()
            {
                Settings = settings
            };
        }

        /// <summary>
        /// Save the given settings.
        /// </summary>
        [HealthCheckModuleMethod(AccessOption.ChangeSettings)]
        public void SetSettings(HealthCheckModuleContext context, SetSettingsViewModel model)
        {
            string settingsString = $"[{string.Join("] [", model.Settings.Select(x => $"{x.Id}: {x.Value}"))}]";
            context.AddAuditEvent(action: "Settings updated", subject: "Settings")
                .AddDetail("Values", settingsString);

            Options.SettingsService.SaveSettings(model.Settings);
        }
        #endregion
    }
}
