using QoDL.Toolkit.Core.Abstractions.Modules;
using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.MappedData.Models;
using QoDL.Toolkit.Core.Modules.MappedData.Utils;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QoDL.Toolkit.Core.Modules.MappedData;

/// <summary>
/// Shows how values are mapped between decorated models.
/// </summary>
public class TKMappedDataModule : ToolkitModuleBase<TKMappedDataModule.AccessOption>
{
    private TKMappedDataModuleOptions Options { get; }

    /// <summary>
    /// Shows how values are mapped between decorated models.
    /// </summary>
    public TKMappedDataModule(TKMappedDataModuleOptions options)
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
    public override object GetFrontendOptionsObject(ToolkitModuleContext context) => null;

    /// <summary>
    /// Get config for this module.
    /// </summary>
    public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => new TKMappedDataModuleConfig();

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
    [ToolkitModuleMethod]
    public TKMappedDataDefinitionsViewModel GetDefinitions(/*ToolkitModuleContext context*/)
    {
        var defs = Options.Service.GetDefinitions(Options.IncludedAssemblies, Options.DiscoveryOptions);
        return Create(defs);
    }

    /// <summary></summary>
    [ToolkitModuleMethod]
    public List<TKMappedExampleValueViewModel> GetExampleValues(/*ToolkitModuleContext context*/)
    {
        var defs = Options.Service.GetDefinitions(Options.IncludedAssemblies, Options.DiscoveryOptions);
        var exampleData = TKMappedDataUtils.GetExampleData();
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
                        var val = TKReflectionUtils.GetValue(example.Instance, x.FullPropertyPath);
                        if (TKMappedDataUtils.ExampleDataValueTransformer != null)
                        {
                            val = TKMappedDataUtils.ExampleDataValueTransformer(val);
                        }

                        var type = val?.GetType();
                        try
                        {
                            if (type != null) val = TransformExampleValue(type, val);
                        }
                        catch (Exception) { }

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

    private static TKMappedDataDefinitionsViewModel Create(TKMappedDataDefinitions d)
    {
        var classDefs = d.ClassDefinitions.Select(x => Create(x)).ToList();
        var refDefs = d.ReferencedDefinitions.Select(x => Create(x)).ToList();
        return new TKMappedDataDefinitionsViewModel
        {
            ClassDefinitions = classDefs,
            ReferencedDefinitions = refDefs
        };
    }

    private static TKMappedReferencedTypeDefinitionViewModel Create(TKMappedReferencedTypeDefinition d)
    {
        return new TKMappedReferencedTypeDefinitionViewModel
        {
            Id = d.Id,
            ReferenceId = d.ReferenceId,
            DisplayName = d.DisplayName,
            NameInMapping = d.NameInMapping,
            TypeName = d.Type?.Name,
            Remarks = d.Attribute?.Remarks
        };
    }

    private static TKMappedClassDefinitionViewModel Create(TKMappedClassDefinition d)
    {
        var memberDefs = d.MemberDefinitions.Select(x => Create(x)).ToList();
        if (d.Attribute?.HtmlEncodeMappingComments == true)
        {
            memberDefs.ForEach(x => x.Remarks = HttpUtility.HtmlEncode(x.Remarks ?? string.Empty));
        }
        return new TKMappedClassDefinitionViewModel
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

    private static TKMappedMemberDefinitionViewModel Create(TKMappedMemberDefinition d)
    {
        var children = d.Children.Select(x => Create(x)).ToList();
        var mappedTo = d.MappedTo.Select(x => Create(x)).ToList();
        return new TKMappedMemberDefinitionViewModel
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

    private static TKMappedMemberReferenceDefinitionViewModel Create(TKMappedMemberReferenceDefinition d)
    {
        var items = d.Items.Select(x => Create(x)).ToList();
        return new TKMappedMemberReferenceDefinitionViewModel
        {
            Success = d.Success,
            Error = d.Error,
            Items = items,
            Path = d.Path,
            RootReferenceId = d.RootReferenceId,
            RootTypeName = d.RootType?.Name
        };
    }

    private static TKMappedMemberReferencePathItemDefinitionViewModel Create(TKMappedMemberReferencePathItemDefinition d)
    {
        return new TKMappedMemberReferencePathItemDefinitionViewModel
        {
            Success = d.Success,
            Error = d.Error,
            DisplayName = d.DisplayName,
            HardCodedValue = d.HardCodedValue,
            DeclaringTypeName = d.DeclaringType?.GetFriendlyTypeName(),
            PropertyName = d.PropertyName,
            PropertyTypeName = d.PropertyType?.GetFriendlyTypeName(),
            FullPropertyTypeName = d?.PropertyType == null ? null : $"{d?.PropertyType?.Namespace}.{d?.PropertyType?.GetFriendlyTypeName()}",
        };
    }
    #endregion
}
