using System.Collections.Generic;
using System.Reflection;

namespace HealthCheck.Core.Modules.Diagrams.FlowCharts
{
    internal class FlowChartAttributeGroup
    {
        public string DiagramId { get; set; }
        public MethodInfo Method { get; set; }
        public List<FlowChartStepAttribute> Attributes { get; set; }
    }
}
