using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Comparison.Abstractions;
using System.Collections.Generic;

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
