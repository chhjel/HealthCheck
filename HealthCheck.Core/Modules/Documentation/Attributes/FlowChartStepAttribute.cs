using HealthCheck.Core.Modules.Documentation.Models.FlowCharts;
using HealthCheck.Core.Modules.Documentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.Documentation.Attributes
{
    /// <summary>
    /// This attribute will be detected by <see cref="DefaultFlowChartService"/> and used in a generated flow chart.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class FlowChartStepAttribute : Attribute
    {
        /// <summary>
        /// Name of the chart.
        /// </summary>
        public string ChartName { get; private set; }

        /// <summary>
        /// Current node title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Connection to other nodes.
        /// <para>Format with label: "Label => Target node"</para>
        /// <para>Format without label: "Target node"</para>
        /// </summary>
        public string[] Connections { get; }

        /// <summary>
        /// Element type to display.
        /// <para>Defaults to selecting type based on title and connections.</para>
        /// </summary>
        public FlowChartStepType? Type { get; set; }

        /// <summary>
        /// Optional category to place this chart in.
        /// </summary>
        public string UICategory { get; set; }

        internal List<string> Branches { get; private set; }
        internal string ClassName { get; set; }
        internal string MethodName { get; set; }
        internal Type Class { get; set; }
        internal MethodInfo Method { get; set; }

        /// <summary>
        /// This attribute will be detected by <see cref="DefaultFlowChartService"/> and used in a generated sequence chart.
        /// </summary>
        /// <param name="chartName">Name of the chart.</param>
        /// <param name="title">Current node title.</param>
        /// <param name="connection">Connection to other node.
        /// <para>Format with label: "Label => Target node"</para>
        /// <para>Format without label: "Target node"</para></param>
        /// <param name="connections">Connection to other nodes.
        /// <para>Format with label: "Label => Target node"</para>
        /// <para>Format without label: "Target node"</para></param>
        /// <param name="uiCategory">Optional category to place this chart in.</param>
        /// <param name="branches">
        /// Any extra branch names given here will result in a new chart containing all the
        /// <para>steps in <paramref name="chartName"/> + any steps with the same branch name.</para>
        /// </param>
        /// <param name="includeInMainBranch">
        /// If set to true the main branch (<paramref name="chartName"/>) will include this step.
        /// <para>Only has effect if <paramref name="branches"/> is given.</para>
        /// </param>
        /// <param name="type">Type of the node (<see cref="FlowChartStepType"/>). Defaults to selecting type based on title and connections.</param>
        public FlowChartStepAttribute(
            string chartName,
            string title,
            string connection = null,
            string[] connections = null,
            string uiCategory = null,
            string[] branches = null,
            bool includeInMainBranch = false,
            object type = null
        )
        {
            ChartName = chartName;
            Title = title;

            Branches = branches?.ToList() ?? new List<string>();
            if (includeInMainBranch || !Branches.Any())
            {
                Branches.Add(chartName);
            }

            Connections = connections ?? new string[0];
            if (connection != null)
            {
                Connections = Connections.Union(new[] { connection }).ToArray();
            }

            UICategory = uiCategory;
            if (type is FlowChartStepType stepType)
            {
                Type = stepType;
            }
        }
    }
}
