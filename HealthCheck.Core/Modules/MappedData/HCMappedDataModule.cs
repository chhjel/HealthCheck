using HealthCheck.Core.Abstractions.Modules;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.MappedData.Models;
using HealthCheck.Core.Modules.MappedData.Utils;
using HealthCheck.Core.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        /// <summary></summary>
        [HealthCheckModuleMethod]
        public List<HCMappedExampleValueViewModel> GetExampleValues(/*HealthCheckModuleContext context*/)
        {
            var defs = Options.Service.GetDefinitions(Options.IncludedAssemblies, Options.DiscoveryOptions);
            var exampleData = HCMappedDataUtils.GetExampleData();
            var examples = defs.ClassDefinitions.Select(x =>
            {
                var example = exampleData.FirstOrDefault(e => e.ClassType == x.ClassType);
                if (example == null) return null;

                if (example.Values == null && example.Instance != null)
                {
                    example.Values = x.AllMemberDefinitions
                        .Where(x => !x.Children.Any())
                        .Select(x =>
                        {
                            var val = HCReflectionUtils.GetValue(example.Instance, x.FullPropertyPath);
                            if (HCMappedDataUtils.ExampleDataValueTransformer != null)
                            {
                                val = HCMappedDataUtils.ExampleDataValueTransformer(val);
                            }

                            var type = val?.GetType();
                            try
                            {
                                if (type != null) val = TransformExampleValue(type, val);
                            }
                            catch (Exception) {}

                            return (value: val?.ToString(), prop: x.FullPropertyPath);
                        })
                        .ToDictionaryIgnoreDuplicates(x => x.prop, x => x.value);

                    example.Instance = null;
                }

                return example;
            }).Where(x => x != null).ToList();
            return examples;
        }
        #endregion

        #region Helpers
        private static object TransformExampleValue(Type type, object obj)
        {
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var suffix = string.Empty;
                var items = (obj as IEnumerable).OfType<object>().Take(100).ToArray();
                if (items.Length == 100)
                {
                    suffix = ", ...";
                }
                return $"[{string.Join(", ", items.Select(x => x.ToString()))}{suffix}]";
            }

            return obj;
        }

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
                NameInMapping = d.NameInMapping,
                TypeName = d.Type?.Name,
                Remarks = d.Attribute?.Remarks
            };
        }

        private static HCMappedClassDefinitionViewModel Create(HCMappedClassDefinition d)
        {
            var memberDefs = d.MemberDefinitions.Select(x => Create(x)).ToList();
            if (d.Attribute?.HtmlEncodeMappingComments == true)
            {
                memberDefs.ForEach(x => x.Remarks = HttpUtility.HtmlEncode(x.Remarks ?? string.Empty));
            }
            return new HCMappedClassDefinitionViewModel
            {
                Id = d.Id,
                ClassTypeName = d.ClassType?.Name,
                DisplayName = d.DisplayName,
                TypeName = d.TypeName,
                MemberDefinitions = memberDefs,
                Remarks = d.Attribute?.Remarks ?? string.Empty,
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
                PropertyTypeName = d.Member?.PropertyType?.Name,
                FullPropertyTypeName = d.Member?.PropertyType == null ? null : $"{d.Member?.PropertyType?.Namespace}.{d.Member?.PropertyType?.Name}",
                PropertyName = d.PropertyName,
                Remarks = d.Remarks,
                IsValid = d.IsValid,
                Error = d.Error,
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
                Success = d.Success,
                Error = d.Error,
                DisplayName = d.DisplayName,
                HardCodedValue = d.HardCodedValue,
                DeclaringTypeName = d.DeclaringType?.GetFriendlyTypeName(),
                PropertyName = d.PropertyName,
                PropertyTypeName =  d.PropertyType?.GetFriendlyTypeName(),
                FullPropertyTypeName = d?.PropertyType == null ? null : $"{d?.PropertyType?.Namespace}.{d?.PropertyType?.GetFriendlyTypeName()}",
            };
        }
        #endregion
    }
}
