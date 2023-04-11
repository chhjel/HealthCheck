namespace QoDL.Toolkit.Core.Modules.Documentation.Models.FlowCharts
{
    /// <summary>
    /// Type of <see cref="FlowChartStep"/>.
    /// </summary>
    public enum FlowChartStepType
    {
        /// <summary>
        /// Default basic element.
        /// </summary>
        Element = 0,

        /// <summary>
        /// If/condition node where choices should appear from.
        /// <para>Defaults if step title ends with '?'</para>
        /// </summary>
	    If,

        /// <summary>
        /// A start of the chart.
        /// <para>Default if no other nodes point to this one.</para>
        /// </summary>
	    Start,

        /// <summary>
        /// An end of the chart.
        /// <para>Default if this node doesn't point to any other nodes.</para>
        /// </summary>
	    End
    }
}
