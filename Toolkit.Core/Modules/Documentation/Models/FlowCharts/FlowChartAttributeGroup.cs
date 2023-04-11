using QoDL.Toolkit.Core.Modules.Documentation.Attributes;
using System.Collections.Generic;
using System.Reflection;

namespace QoDL.Toolkit.Core.Modules.Documentation.Models.FlowCharts
{
    internal class FlowChartAttributeGroup
    {
        public string DiagramId { get; set; }
        public MethodInfo Method { get; set; }
        public List<FlowChartStepAttribute> Attributes { get; set; }
    }
}
