using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QoDL.Toolkit.Core.Modules.Tests.Utils.HtmlPresets;

/// <summary>
/// Builds html from <see cref="IHtmlPreset"/>.
/// </summary>
public class HtmlPresetBuilder : IHtmlPreset
{
    /// <summary>
    /// Builds html from presets when added.
    /// </summary>
    protected StringBuilder HtmlBuilder { get; set; } = new StringBuilder();

    /// <summary>
    /// Builds html from <see cref="IHtmlPreset"/>.
    /// </summary>
    public HtmlPresetBuilder() { }

    /// <summary>
    /// Builds html from <see cref="IHtmlPreset"/>.
    /// <para>This overload inserts a raw html item. Similar to <c>builder.AddItem(new HtmlPresetRaw(html))</c></para>
    /// </summary>
    public HtmlPresetBuilder(string html)
    {
        AddItem(new HtmlPresetRaw(html));
    }

    /// <summary>
    /// Add some raw html.
    /// </summary>
    public HtmlPresetBuilder AddRawHtml(string html)
    {
        if (!string.IsNullOrWhiteSpace(html))
        {
            HtmlBuilder.AppendLine(html);
        }
        return this;
    }

    /// <summary>
    /// Add a html-preset object.
    /// <para>
    /// Built in presets:
    /// <see cref="HtmlPresetHeader"/>,
    /// <see cref="HtmlPresetIFrame"/>,
    /// <see cref="HtmlPresetImage"/>,
    /// <see cref="HtmlPresetKeyValueList"/>,
    /// <see cref="HtmlPresetList"/>,
    /// <see cref="HtmlPresetProgressbar"/>,
    /// <see cref="HtmlPresetRaw"/>,
    /// <see cref="HtmlPresetSpace"/>,
    /// <see cref="HtmlPresetText"/>
    /// </para>
    /// </summary>
    public HtmlPresetBuilder AddItem(IHtmlPreset preset)
    {
        if (preset != null)
        {
            HtmlBuilder.AppendLine(preset.ToHtml());
        }
        return this;
    }

    /// <summary>
    /// Add several html-presets.
    /// <para>
    /// Built in presets:
    /// <see cref="HtmlPresetHeader"/>,
    /// <see cref="HtmlPresetIFrame"/>,
    /// <see cref="HtmlPresetImage"/>,
    /// <see cref="HtmlPresetKeyValueList"/>,
    /// <see cref="HtmlPresetList"/>,
    /// <see cref="HtmlPresetProgressbar"/>,
    /// <see cref="HtmlPresetRaw"/>,
    /// <see cref="HtmlPresetSpace"/>,
    /// <see cref="HtmlPresetText"/>
    /// </para>
    /// </summary>
    public HtmlPresetBuilder AddItems(IEnumerable<IHtmlPreset> presets)
    {
        if (presets != null)
        {
            foreach (var preset in presets.Where(x => x != null))
            {
                AddItem(preset);
            }
        }

        return this;
    }

    /// <summary>
    /// Add several html-presets.
    /// <para>
    /// Built in presets:
    /// <see cref="HtmlPresetHeader"/>,
    /// <see cref="HtmlPresetIFrame"/>,
    /// <see cref="HtmlPresetImage"/>,
    /// <see cref="HtmlPresetKeyValueList"/>,
    /// <see cref="HtmlPresetList"/>,
    /// <see cref="HtmlPresetProgressbar"/>,
    /// <see cref="HtmlPresetRaw"/>,
    /// <see cref="HtmlPresetSpace"/>,
    /// <see cref="HtmlPresetText"/>
    /// </para>
    /// </summary>
    public HtmlPresetBuilder AddItems(params IHtmlPreset[] presets)
        => AddItems(presets.ToList());

    /// <summary>
    /// Build html from all the items added.
    /// </summary>
    /// <returns></returns>
    public string ToHtml() => HtmlBuilder.ToString();
}