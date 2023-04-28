using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Core.Config;
using QoDL.Toolkit.Core.Modules.DataRepeater.Abstractions;
using QoDL.Toolkit.Core.Modules.Metrics.Context;
using QoDL.Toolkit.Module.DataExport;
using QoDL.Toolkit.Module.DynamicCodeExecution.Module;
using QoDL.Toolkit.Module.EndpointControl.Abstractions;
using QoDL.Toolkit.Module.EndpointControl.Module;
using QoDL.Toolkit.Module.IPWhitelist.Module;
using QoDL.Toolkit.WebUI.Models;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace QoDL.Toolkit.FrontendModelGenerator
{
    public static class RTConfig
    {
        public static void Configure(ConfigurationBuilder builder)
        {
            builder.Global((config) =>
            {
                // config.CamelCaseForProperties() todo, convert frontend to consume it
                config.UseModules(true);
            });

            // todo: only include needed
            builder.Substitute(typeof(Guid), new RtSimpleTypeName("string"));
            builder.Substitute(typeof(Type), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(MethodInfo), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(PropertyInfo), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(Regex), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(TimeSpan), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(KeyValuePair<,>), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(Stream), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(IEndpointControlRequestResult), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(ActionResult), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(HttpResponseMessage), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(Exception), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(ITKDataRepeaterStreamItem), new RtSimpleTypeName("any"));
            builder.Substitute(typeof(DateTime), new RtSimpleTypeName("Date"));
            builder.Substitute(typeof(DateTimeOffset), new RtSimpleTypeName("Date"));
            builder.Substitute(typeof(List<KeyValuePair<string, string>>), new RtSimpleTypeName("{ [key: string] : string; }"));
            builder.Substitute(typeof(List<KeyValuePair<string, Guid>>), new RtSimpleTypeName("{ [key: string] : string; }"));

            var extraTypes = new[] { typeof(TKMetricsContext), typeof(TKFrontEndOptions.TKUserModuleCategories) };
            builder.ExportAsInterfaces(extraTypes.Where(x => !x.IsEnum), (config) => ConfigureInterfaces(config, typeof(TKGlobalConfig).Assembly));

            IncludeAssembly(builder, typeof(TKGlobalConfig).Assembly);
            IncludeAssembly(builder, typeof(TKPageOptions).Assembly);
            IncludeAssembly(builder, typeof(TKEndpointControlModule).Assembly);
            IncludeAssembly(builder, typeof(TKDynamicCodeExecutionModule).Assembly);
            IncludeAssembly(builder, typeof(TKDataExportModule).Assembly);
            IncludeAssembly(builder, typeof(TKIPWhitelistModule).Assembly);
        }

        private static void IncludeAssembly(ConfigurationBuilder builder, Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(x => (x.Namespace.StartsWith("QoDL.Toolkit")) && !x.IsGenericType)
                .ToArray();

            builder.ExportAsEnums(types.Where(x => x.IsEnum
                && (x.Namespace.Contains(".Models") || x.Namespace.Contains(".Enums"))),
                (config) => ConfigureEnums(config, assembly));
            builder.ExportAsInterfaces(types.Where(x => !x.IsEnum && x.Namespace.Contains(".Models")),
                (config) => ConfigureInterfaces(config, assembly));
        }

        private static void ConfigureInterfaces(InterfaceExportBuilder config, Assembly assembly)
        {
            config.AutoI(false);
            config.OverrideNamespace(CreateNamespace("Models.", assembly));
            config.WithAllMethods(m => m.Ignore());
            config.WithAllProperties((c) =>
            {
                if (c.Member.GetCustomAttributes().FirstOrDefault(x => x is TKRtPropertyAttribute) is TKRtPropertyAttribute attr)
                {
                    if (attr.ForcedNullable)
                    {
                        c.ForceNullable(true);
                    }
                    if (!string.IsNullOrWhiteSpace(attr.ForcedType))
                    {
                        c.Type(attr.ForcedType);
                    }
                }
            });
        }

        private static void ConfigureEnums(EnumExportBuilder config, Assembly assembly)
        {
            config.UseString(true);
            config.OverrideNamespace(CreateNamespace("Enums.", assembly));
        }

        private static string CreateNamespace(string prefix, Assembly assembly)
            => $"{prefix}{(assembly?.GetName()?.Name?.Replace("QoDL.Toolkit.", "")?.Replace("Toolkit.", "") ?? "Other")}";
    }
}
