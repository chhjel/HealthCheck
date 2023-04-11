using QoDL.Toolkit.Core.Modules.Documentation.Models.SequenceDiagrams;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.Documentation.Util.ThirdParty
{
    /// <summary>
    /// Helper for https://www.websequencediagrams.com/
    /// </summary>
    public static class WebSequenceDiagramHelper
    {
        /// <summary>
        /// Generate a string that can be copied into https://www.websequencediagrams.com/.
        /// </summary>
        public static string GenerateDiagramCode(SequenceDiagram diagram)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"title {diagram.Name}");

            string currentOptionalGroup = null;
            foreach (var step in diagram.Steps)
            {
                if (step.OptionalGroupName != currentOptionalGroup && currentOptionalGroup != null)
                {
                    builder.AppendLine($"end");
                    currentOptionalGroup = null;
                }

                if (step.OptionalGroupName != null && step.OptionalGroupName != currentOptionalGroup)
                {
                    builder.AppendLine($"opt {step.OptionalGroupName}");
                    currentOptionalGroup = step.OptionalGroupName;
                }

                if (step.Note != null)
                {
                    builder.AppendLine($"note right of {step.From}");
                    builder.AppendLine(step.Note);
                    builder.AppendLine($"end note");
                }

                var arrow = step.Direction == SequenceDiagramStepDirection.Backward ? "-->" : "->";
                builder.AppendLine($"{step.From} {arrow} {step.To}: {step.Description}");
            }

            if (currentOptionalGroup != null)
            {
                builder.AppendLine($"end");
            }

            return builder.ToString();
        }
    }
}
