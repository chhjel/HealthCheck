using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Modules.MappedData.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.MappedData
{
    /// <summary>
    /// Shows how values are mapped between decorated models.
    /// </summary>
    public class HCMappedDataModule : HealthCheckModuleBase<HCMappedDataModule.AccessOption>
    {
        private HCMappedDataModuleOptions Options { get; }

        /// <summary>
        /// Shows how values are mapped between decorated models.
        /// </summary>
        public HCMappedDataModule(HCMappedDataModuleOptions options)
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
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => new HCMappedDataModuleConfig();
        
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
        /// <summary></summary>
        [HealthCheckModuleMethod]
        public List<HCMappedClassesDefinitionViewModel> GetDefinitions(/*HealthCheckModuleContext context*/)
        {
            var pairs = Options.Service.GetDefinitions(Options.IncludedAssemblies, Options.DiscoveryOptions);
            return pairs.Select(x => Create(x)).ToList();
        }
        #endregion

        #region Helpers
        private static HCMappedClassesDefinitionViewModel Create(HCMappedClassesDefinition d)
        {
            var members = d.MemberPairs.Select(x => Create(x)).ToList();
            return new HCMappedClassesDefinitionViewModel
            {
                GroupName = d.GroupName,
                Left = Create(d.Left),
                Right = Create(d.Right),
                MemberPairs = members
            };
        }

        private static HCMappedClassDefinitionViewModel Create(HCMappedClassDefinition d)
        {
            var memberDefs = d.MemberDefinitions.Select(x => Create(x)).ToList();
            return new HCMappedClassDefinitionViewModel
            {
                Id = d.Id,
                ClassTypeName = d.ClassType.Name,
                DisplayName = d.DisplayName,
                TypeName = d.TypeName,
                MapsToDefinitionId = d.MapsToDefinitionId,
                MemberDefinitions = memberDefs,
                Remarks = d.Attribute?.Remarks,
                DataSourceName = d.Attribute?.DataSourceName,
                GroupName = d.Attribute?.GroupName
            };
        }

        private static HCMappedMemberDefinitionPairViewModel Create(HCMappedMemberDefinitionPair d)
        {
            return new HCMappedMemberDefinitionPairViewModel
            {
                Left = Create(d.Left),
                Right = Create(d.Right)
            };
        }

        private static HCMappedMemberDefinitionViewModel Create(HCMappedMemberDefinition d)
        {
            return new HCMappedMemberDefinitionViewModel
            {
                Id = d.Id,
                DisplayName = d.DisplayName,
                FullTypeName = d.FullTypeName,
                PropertyName = d.PropertyName,
                Remarks = d.Attribute?.Remarks
            };
        }
        #endregion
    }
}
