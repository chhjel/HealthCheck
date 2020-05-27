using HealthCheck.Core.Modules.Documentation.Abstractions;
using HealthCheck.Core.Modules.Documentation.Attributes;
using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.Documentation.Services
{
    /// <summary>
    /// Generates flow chart data from <see cref="FlowChartStepAttribute"/>s.
    /// </summary>
    public class DefaultFlowChartService : IFlowChartsService
    {
        private DefaultFlowChartServiceOptions Options { get; }

        /// <summary>
        /// Generates flow chart data from <see cref="FlowChartStepAttribute"/>s.
        /// </summary>
        public DefaultFlowChartService(DefaultFlowChartServiceOptions options)
        {
            Options = options ?? new DefaultFlowChartServiceOptions();
        }

        /// <summary>
        /// Generates flow chart data from <see cref="FlowChartStepAttribute"/>s in the given assemblies.
        /// </summary>
        public List<FlowChart> Generate(params Assembly[] sourceAssemblies)
            => Generate(sourceAssemblies.ToList());

        /// <summary>
        /// Generates flow chart data from <see cref="FlowChartStepAttribute"/>s in the given assembly.
        /// </summary>
        public List<FlowChart> Generate(Assembly sourceAssembly)
            => Generate(new List<Assembly> { sourceAssembly });

        /// <summary>
        /// Generates flow chart data from <see cref="FlowChartStepAttribute"/>s in the given assemblies.
        /// </summary>
        public List<FlowChart> Generate(IEnumerable<Assembly> sourceAssemblies = null)
        {
            sourceAssemblies ??= Options.DefaultSourceAssemblies
                ?? Enumerable.Empty<Assembly>();

            var attributeGroups = FindAttributes(sourceAssemblies);
            var diagrams = new List<FlowChart>();
            foreach (var group in attributeGroups)
            {
                var groupDiagrams = group.Attributes
                    .SelectMany(x => x.Branches)
                    .Distinct()
                    .ToDictionary(x => x, x => new FlowChart()
                    {
                        Name = x
                    });

                foreach (var kvp in groupDiagrams)
                {
                    var diagram = kvp.Value;
                    var attributes = group.Attributes
                        .Where(x => x.Branches.Contains(diagram.Name) || x.Branches.Contains(x.ChartName))
                        .ToArray();

                    for (int i = 0; i < attributes.Length; i++)
                    {
                        var atr = attributes[i];

                        var connections = atr.Connections
                            .Select(x =>
                            {
                                var parts = x.Split(new[] { "=>" }, 2, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length < 2)
                                {
                                    return new FlowChartConnection
                                    {
                                        Target = x.Trim()
                                    };
                                }

                                return new FlowChartConnection
                                {
                                    Label = parts[0].Trim(),
                                    Target = parts[1].Trim()
                                };
                            }).Where(x => x != null).ToList();

                        var step = new FlowChartStep()
                        {
                            Title = atr.Title,
                            Type = atr.Type,
                            Connections = connections,
                            ClassName = atr.Class?.FullName,
                            MethodName = atr.Method?.ToString()
                        };

                        diagram.Steps.Add(step);
                    }
                }

                diagrams.AddRange(groupDiagrams
                    .Where(x => x.Value.Steps.Count >= 1)
                    .Select(x => x.Value)
                );
            }

            return diagrams;
        }

        private List<FlowChartAttributeGroup> FindAttributes(IEnumerable<Assembly> sourceAssemblies)
        {
            var types = sourceAssemblies.SelectMany(x => x.DefinedTypes).ToArray();
            var methods = types.SelectMany(x => x.GetMethods()).ToList();
            var methodAttributes = methods
                .Where(x => x.GetCustomAttributes<FlowChartStepAttribute>().Count() > 0)
                .Select(x => new
                {
                    Method = x,
                    Attributes = x.GetCustomAttributes<FlowChartStepAttribute>().ToList()
                })
                .ToList();

            var groups = new Dictionary<string, FlowChartAttributeGroup>();
            foreach (var method in methodAttributes)
            {
                for (int i = 0; i < method.Attributes.Count; i++)
                {
                    var atr = method.Attributes[i];
                    FlowChartAttributeGroup group = null;
                    if (groups.ContainsKey(atr.ChartName))
                    {
                        group = groups[atr.ChartName];
                    }
                    else
                    {
                        group = new FlowChartAttributeGroup()
                        {
                            DiagramId = atr.ChartName,
                            Method = method.Method,
                            Attributes = new List<FlowChartStepAttribute>()
                        };
                        groups[atr.ChartName] = group;
                    }

                    atr.Class = method.Method.DeclaringType;
                    atr.ClassName = method.Method.DeclaringType.Name;
                    atr.Method = method.Method;
                    atr.MethodName = method.Method.Name;

                    group.Attributes.Add(atr);
                }
            }

            return groups
                .Select(x => x.Value)
                .Where(x => x.Attributes.Count > 0)
                .ToList();
        }
    }
}
