#if NETFULL || NETCORE
using HealthCheck.Module.DynamicCodeExecution.Abstractions;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Module.DynamicCodeExecution.PreProcessors
{
    /// <summary>
    /// Wraps the first batch of using statements found in a region.
    /// </summary>
    public class WrapUsingsInRegionPreProcessor : IDynamicCodePreProcessor
    {
        /// <summary>
        /// Id of the pre-processor used to disable it from the code.
        /// </summary>
        public string Id { get; set; } = "WrapUsingsInRegion";

        /// <summary>
        /// Optional title returned in the options model.
        /// </summary>
        public string Name { get; set; } = "Wrap usings in region";

        /// <summary>
        /// Optional description returned in the options model.
        /// </summary>
        public string Description { get; set; } = "Automatically creates a region around using statements.";

        /// <summary>
        /// Allow this pre-processor to be disabled by the user?
        /// </summary>
        public bool CanBeDisabled { get; set; } = true;

        /// <summary>
        /// Name of the region to create if missing.
        /// </summary>
        public string RegionName { get; set; } = "Usings";

        /// <summary>
        /// Remove empty lines between usings statements.
        /// </summary>
        public bool RemoveEmptyLines { get; set; } = true;

        /// <summary>
        /// Wraps the first batch of using statements found in a region.
        /// </summary>
        public string PreProcess(CompilerParameters options, string code)
        {
            if (code == null || code.Trim().Length == 0)
                return code;

            return WrapUsingsInRegion("Usings", code);
        }

        private string WrapUsingsInRegion(string name, string code)
        {
            var lines = code.Split('\n').ToList();
            if (!lines.Any(x => x.Trim().StartsWith("using ")))
            {
                return code;
            }

            var regionContent = ExtractAllUsingLines(lines);
            var topComments = regionContent
                .TakeWhile(x => x.Trim().Length == 0 || x.Trim().StartsWith("//"))
                .ToList();
            regionContent = regionContent.Skip(topComments.Count).ToList();

            var regionLineIndex = lines.FindIndex(x => x.Trim() == $"#region {name}");
            if (regionLineIndex == -1)
            {
                var newLines = new List<string>
                {
                    $"#region {name}"
                };
                newLines.AddRange(regionContent);
                newLines.Add($"#endregion");
                newLines.Add("");
                lines.InsertRange(0, newLines);
            }
            else
            {
                lines.InsertRange(regionLineIndex + 1, regionContent);
                lines.Insert(regionLineIndex + 2 + regionContent.Count, "");
                var isLineBelowEndRegion = false;
                for(int i = 0; i < lines.Count; i++)
                {
                    // Ensure empty line below #endregion
                    if (isLineBelowEndRegion)
                    {
                        if (lines[i].Trim().Length > 0)
                        {
                            lines.Insert(i, "");
                        }
                        break;
                    }
                    else if(lines[i].Trim() == "#endregion")
                    {
                        isLineBelowEndRegion = true;
                    }
                    else if(lines[i].Trim().Length == 0)
                    {
                        lines.RemoveAt(i);
                        i--;
                    }
                }
            }
            
            lines.InsertRange(0, topComments);

            return string.Join("\n", lines);
        }

        private List<string> ExtractAllUsingLines(List<string> lines)
        {
            var extractedLines = new List<string>();
            for(int i=0;i<lines.Count;i++)
            {
                var remove = false;
                var line = lines[i];
                var lineIsAllowedWithinUsingsRegion = line.Trim().StartsWith("using ") || line.Trim().Length == 0 || line.Trim().StartsWith("//");
                if (!lineIsAllowedWithinUsingsRegion)
                {
                    break;
                }

                if (line.Trim().Length > 0 || !RemoveEmptyLines)
                {
                    extractedLines.Add(line);
                    remove = true;
                }
                else if(RemoveEmptyLines)
                {
                    remove = true;
                }

                if (remove)
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }
            return extractedLines;
        }

    }
}
#endif
