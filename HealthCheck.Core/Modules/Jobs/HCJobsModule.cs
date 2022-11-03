using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class HCJobsModule : HealthCheckModuleBase<HCJobsModule.AccessOption>
    {
        private HCJobsModuleOptions Options { get; }

        /// <summary>
        /// 
        /// </summary>
        public HCJobsModule(HCJobsModuleOptions options)
        {
            Options = options;
        }

        /// <summary>
        /// Check options object for issues.
        /// </summary>
        public override IEnumerable<string> Validate()
        {
            var issues = new List<string>();
            if (Options.Service == null) issues.Add("Options.Service must be set.");
            return issues;
        }

        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCJobsModuleConfig();
        
        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum AccessOption
        {
            /// <summary>Does nothing.</summary>
            None = 0
        }

        #region Invokable methods
        ///// <summary></summary>
        //[HealthCheckModuleMethod]
        //public Task<object> GetSomething(/*HealthCheckModuleContext context*/)
        //{
        //    return Task.FromResult(data);
        //}

        /*
        Task<List<HCJobDefinitionWithSource>> GetJobDefinitions();
        Task<List<HCJobHistoryEntry>> GetPagedHistoryAsync(string jobId, int pageIndex, int pageSize);
        Task<List<HCJobHistoryEntry>> GetLatestHistoryPerJobIdAsync();
        Task<HCJobHistoryDetailEntry> GetHistoryDetailAsync(Guid id);
         */
        #endregion
    }
}