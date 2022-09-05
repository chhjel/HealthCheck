using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Comparison.Abstractions;
using HealthCheck.Core.Modules.Tests.Utils.HtmlPresets;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HealthCheck.Core.Modules.Comparison.Models
{
    /// <summary>
    /// Output from <see cref="IHCComparisonDiffer.CompareInstancesAsync"/>
    /// </summary>
    public class HCComparisonDifferOutput
    {
        /// <summary>
        /// Diff output to display.
        /// </summary>
        internal List<HCComparisonDifferOutputData> Data { get; set; } = new();

        /// <summary>
        /// True if no data has been added.
        /// </summary>
        public bool IsEmpty => Data.Count == 0;

        /// <summary>
        /// Add dictionary data.
        /// <para>Compares left and right data with the same keys and highlights differences.</para>
        /// </summary>
        public HCComparisonDifferOutput AddDictionaryData(Dictionary<string, string> leftData, Dictionary<string, string> rightData, string title, string leftSideTitle = null, string rightSideTitle = null,
            bool onlyIncludeDifferences = false, bool prettifyKeys = true)
        {
            leftData ??= new Dictionary<string, string>();
            rightData ??= new Dictionary<string, string>();
            
            // Compute diff
            var allKeys = leftData.Keys.Union(rightData.Keys).ToArray();
            var resultingDictionary = new Dictionary<string, (string, string, bool)>();
            foreach (var key in allKeys)
            {
                leftData.TryGetValue(key, out var leftValue);
                rightData.TryGetValue(key, out var rightValue);
                var matches = leftValue == rightValue;
                resultingDictionary[key] = (leftValue, rightValue, matches);
            }

            // Build html
            var diffCount = 0;
            var builder = new StringBuilder();
            builder.AppendLine(@"
<style>
.diff-align-right {
    text-align: right;
}
.diff-matches {
    background-color: var(--color--success-base) !important;
    color: #fff;
    font-size: 14px;
}
.diff-differs {
    background-color: var(--color--error-lighten3) !important;
    color: #fff;
}
.diff-hidden {
    visibility: hidden;
}
</style>");

            builder.AppendLine($"<table class=\"table\" style=\"margin:auto\">");
            builder.AppendLine($" <tr>");
            builder.AppendLine($"  <th class=\"diff-align-right\">Property</th>");
            builder.AppendLine($"  <th class=\"diff-align-right\">{(leftSideTitle ?? "Left")}</th>");
            builder.AppendLine($"  <th>{(rightSideTitle ?? "Right")}</th>");
            builder.AppendLine($"  <th class=\"diff-hidden\">Property</th>");
            builder.AppendLine($" </tr>");
            foreach (var kvp in resultingDictionary)
            {
                var key = prettifyKeys ? kvp.Key.SpacifySentence() : kvp.Key;
                var matches = kvp.Value.Item3;
                if (onlyIncludeDifferences && matches) continue;
                if (!matches) diffCount++;

                builder.AppendLine($" <tr class=\"{(matches ? "diff-matches" : "diff-differs")}\">");
                builder.AppendLine($"  <td class=\"diff-align-right\">{key}</td>");
                builder.AppendLine($"  <td class=\"diff-align-right\">{kvp.Value.Item1}</td>");
                builder.AppendLine($"  <td>{kvp.Value.Item2}</td>");
                builder.AppendLine($"  <td class=\"diff-hidden\">{key}</td>");
                builder.AppendLine($" </tr>");
            }
            builder.AppendLine($"</table>");

            var html = (onlyIncludeDifferences && diffCount == 0) ? string.Empty : builder.ToString();
            return AddHtml(html, title);
        }

        /// <summary>
        /// Add custom html.
        /// </summary>
        public HCComparisonDifferOutput AddHtml(HtmlPresetBuilder htmlBuilder, string title)
            => AddHtml(htmlBuilder?.ToHtml() ?? string.Empty, title);

        /// <summary>
        /// Add custom html.
        /// </summary>
        public HCComparisonDifferOutput AddHtml(string html, string title)
        {
            Data.Add(new HCComparisonDifferOutputData
            {
                Title = title,
                DataType = HCComparisonDiffOutputType.Html,
                Data = HCGlobalConfig.Serializer.Serialize(new
                {
                    Html = html
                }, true)
            });
            return this;
        }

        /// <summary>
        /// Add a custom html for each side.
        /// </summary>
        public HCComparisonDifferOutput AddSideHtml(HtmlPresetBuilder leftSideHtml, HtmlPresetBuilder rightSideHtml, string title)
            => AddSideHtml(leftSideHtml?.ToHtml() ?? string.Empty, rightSideHtml?.ToHtml() ?? string.Empty, title);

        /// <summary>
        /// Add a custom html for each side.
        /// </summary>
        public HCComparisonDifferOutput AddSideHtml(string leftSideHtml, string rightSideHtml, string title)
        {
            Data.Add(new HCComparisonDifferOutputData
            {
                Title = title,
                DataType = HCComparisonDiffOutputType.SideHtml,
                Data = HCGlobalConfig.Serializer.Serialize(new
                {
                    Left = leftSideHtml,
                    Right = rightSideHtml
                }, true)
            });
            return this;
        }

        /// <summary>
        /// Add a note.
        /// </summary>
        public HCComparisonDifferOutput AddNote(string note, string title)
        {
            Data.Add(new HCComparisonDifferOutputData
            {
                Title = title,
                DataType = HCComparisonDiffOutputType.Note,
                Data = HCGlobalConfig.Serializer.Serialize(new
                {
                    Note = note
                }, true)
            });
            return this;
        }

        /// <summary>
        /// Add a note for each side.
        /// </summary>
        public HCComparisonDifferOutput AddSideNotes(string leftSideNote, string rightSideNote, string title)
        {
            Data.Add(new HCComparisonDifferOutputData
            {
                Title = title,
                DataType = HCComparisonDiffOutputType.SideNotes,
                Data = HCGlobalConfig.Serializer.Serialize(new
                {
                    Left = leftSideNote,
                    Right = rightSideNote
                }, true)
            });
            return this;
        }

        /// <summary>
        /// Add output diff of two objects to be serialized
        /// </summary>
        public HCComparisonDifferOutput AddDiff(object leftSide, object rightSide, string title, string leftSideTitle = null, string rightSideTitle = null)
        {
            var left = HCGlobalConfig.Serializer.Serialize(leftSide, true);
            var right = HCGlobalConfig.Serializer.Serialize(rightSide, true);
            return AddDiff(left, right, title, leftSideTitle, rightSideTitle);
        }

        /// <summary>
        /// Add output diff of two strings.
        /// </summary>
        public HCComparisonDifferOutput AddDiff(string leftSide, string rightSide, string title, string leftSideTitle, string rightSideTitle)
        {
            var data = new
            {
                OriginalName = leftSideTitle,
                OriginalContent = leftSide,
                ModifiedName = rightSideTitle,
                ModifiedContent = rightSide
            };
            return AddData(title, HCComparisonDiffOutputType.Diff, data);
        }

        private HCComparisonDifferOutput AddData(string title, HCComparisonDiffOutputType type, object data)
        {
            Data.Add(new HCComparisonDifferOutputData
            {
                Title = title,
                DataType = type,
                Data = HCGlobalConfig.Serializer.Serialize(data, false)
            });
            return this;
        }
    }
}
