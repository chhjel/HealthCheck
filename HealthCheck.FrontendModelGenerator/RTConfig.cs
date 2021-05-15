using HealthCheck.Core.Attributes;
using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Metrics.Context;
using HealthCheck.Module.DynamicCodeExecution.Module;
using HealthCheck.Module.EndpointControl.Abstractions;
using HealthCheck.Module.EndpointControl.Module;
using HealthCheck.WebUI.Models;
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

namespace HealthCheck.FrontendModelGenerator
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
            builder.Substitute(typeof(DateTime), new RtSimpleTypeName("Date"));
            builder.Substitute(typeof(DateTimeOffset), new RtSimpleTypeName("Date"));
            builder.Substitute(typeof(List<KeyValuePair<string, string>>), new RtSimpleTypeName("{ [key: string] : string; }"));
            builder.Substitute(typeof(List<KeyValuePair<string, Guid>>), new RtSimpleTypeName("{ [key: string] : string; }"));

            var extraTypes = new[] { typeof(HCMetricsContext) };
            builder.ExportAsInterfaces(extraTypes.Where(x => !x.IsEnum), (config) => ConfigureInterfaces(config, typeof(HCGlobalConfig).Assembly));

            IncludeAssembly(builder, typeof(HCGlobalConfig).Assembly);
            IncludeAssembly(builder, typeof(HCPageOptions).Assembly);
            IncludeAssembly(builder, typeof(HCEndpointControlModule).Assembly);
            IncludeAssembly(builder, typeof(HCDynamicCodeExecutionModule).Assembly);
        }

        private static void IncludeAssembly(ConfigurationBuilder builder, Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(x => x.Namespace.StartsWith("HealthCheck") && !x.IsGenericType)
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
                var attr = c.Member.GetCustomAttributes().FirstOrDefault(x => x is HCRtPropertyAttribute) as HCRtPropertyAttribute;
                if (attr != null)
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
            => $"{prefix}{(assembly?.GetName()?.Name?.Replace("HealthCheck.", "") ?? "Other")}";
    }
}
