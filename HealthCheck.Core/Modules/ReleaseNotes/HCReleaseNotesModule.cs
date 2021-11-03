using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.ReleaseNotes.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthCheck.Core.Modules.ReleaseNotes
{
    /// <summary>
    /// Module for viewing release notes.
    /// </summary>
    public class HCReleaseNotesModule : HealthCheckModuleBase<HCReleaseNotesModule.AccessOption>
    {
        private HCReleaseNotesModuleOptions Options { get; }

        /// <summary>
        /// Module for viewing ReleaseNotes.
        /// </summary>
        public HCReleaseNotesModule(HCReleaseNotesModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.ReleaseNotesProvider == null) issues.Add("Options.ReleaseNotesProvider must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCReleaseNotesModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0,

            /// <summary>Includes git commit messages, changes without issue ids, author and PR links.</summary>
            DeveloperDetails = 1,
        }

        #region Invokable methods
        /// <summary></summary>
        [HealthCheckModuleMethod]
        public async Task<HCReleaseNotesViewModel> GetReleaseNotesWithoutDevDetails()
        {
            var model = await Options.ReleaseNotesProvider.GetViewModelAsync();
            return model?.WithoutDevDetails ?? new HCReleaseNotesViewModel { ErrorMessage = "No release notes found." };
        }

        /// <summary></summary>
        [HealthCheckModuleMethod(AccessOption.DeveloperDetails)]
        public async Task<HCReleaseNotesViewModel> GetReleaseNotesWithDevDetails()
        {
            var model = await Options.ReleaseNotesProvider.GetViewModelAsync();
            return model?.WithDevDetails ?? new HCReleaseNotesViewModel { ErrorMessage = "No release notes found." };
        }
        #endregion
    }
}
