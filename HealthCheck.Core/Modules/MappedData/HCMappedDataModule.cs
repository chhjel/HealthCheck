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
        public HCMappedDataDefinitionsViewModel GetDefinitions(/*HealthCheckModuleContext context*/)
        {
            var defs = Options.Service.GetDefinitions(Options.IncludedAssemblies, Options.DiscoveryOptions);
            return Create(defs);
        }
        #endregion

        #region Helpers
        private static HCMappedDataDefinitionsViewModel Create(HCMappedDataDefinitions d)
        {
            var classDefs = d.ClassDefinitions.Select(x => Create(x)).ToList();
            var refDefs = d.ReferencedDefinitions.Select(x => Create(x)).ToList();
            return new HCMappedDataDefinitionsViewModel
            {
                ClassDefinitions = classDefs,
                ReferencedDefinitions = refDefs
            };
        }

        private static HCMappedReferencedTypeDefinitionViewModel Create(HCMappedReferencedTypeDefinition d)
        {
            return new HCMappedReferencedTypeDefinitionViewModel
            {
                Id = d.Id,
                ReferenceId = d.ReferenceId,
                DisplayName = d.DisplayName,
                TypeName = d.Type?.Name,
                Remarks = d.Attribute?.Remarks
            };
        }

        private static HCMappedClassDefinitionViewModel Create(HCMappedClassDefinition d)
        {
            var memberDefs = d.MemberDefinitions.Select(x => Create(x)).ToList();
            return new HCMappedClassDefinitionViewModel
            {
                Id = d.Id,
                ClassTypeName = d.ClassType?.Name,
                DisplayName = d.DisplayName,
                TypeName = d.TypeName,
                MemberDefinitions = memberDefs,
                Remarks = d.Attribute?.Remarks,
                GroupName = d.Attribute?.GroupName
            };
        }

        private static HCMappedMemberDefinitionViewModel Create(HCMappedMemberDefinition d)
        {
            var children = d.Children.Select(x => Create(x)).ToList();
            var mappedTo = d.MappedTo.Select(x => Create(x)).ToList();
            return new HCMappedMemberDefinitionViewModel
            {
                Id = d.Id,
                DisplayName = d.DisplayName,
                FullPropertyPath = d.FullPropertyPath,
                PropertyName = d.PropertyName,
                Remarks = d.Remarks,
                Children = children,
                MappedTo = mappedTo
            };
        }

        private static HCMappedMemberReferenceDefinitionViewModel Create(HCMappedMemberReferenceDefinition d)
        {
            var items = d.Items.Select(x => Create(x)).ToList();
            return new HCMappedMemberReferenceDefinitionViewModel
            {
                Success = d.Success,
                Error = d.Error,
                Items = items,
                Path = d.Path,
                RootReferenceId = d.RootReferenceId,
                RootTypeName = d.RootType?.Name
            };
        }

        private static HCMappedMemberReferencePathItemDefinitionViewModel Create(HCMappedMemberReferencePathItemDefinition d)
        {
            return new HCMappedMemberReferencePathItemDefinitionViewModel
            {
                DisplayName = d.DisplayName,
                Success = d.Success,
                Error = d.Error,
                DeclaringTypeName = d.DeclaringType?.Name,
                PropertyName = d.PropertyName
            };
        }
        #endregion
    }
}
