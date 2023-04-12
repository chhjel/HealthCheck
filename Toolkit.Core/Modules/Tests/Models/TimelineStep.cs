using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// A step in a timeline data dump.
/// </summary>
public class TimelineStep
{
    /// <summary>
    /// Title of the step.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Description of the step.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Optional url of the step.
    /// </summary>
    public List<HyperLink> Links { get; set; } = new List<HyperLink>();

    /// <summary>
    /// Optional error of the step.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Optional icon name for the step.
    /// <para>Find one at https://material.io/resources/icons/?style=baseline</para>
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Optional timestamp of the step.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// True to only show the date part.
    /// </summary>
    public bool HideTimeInTimestamp { get; set; }

    /// <summary>
    /// If true the step will be marked as completed.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// A step in a timeline data dump.
    /// </summary>
    public TimelineStep(string title, string description = null,
        DateTimeOffset? timestamp = null, bool hideTimeInTimestamp = false, string icon = null,
        string error = null, bool isCompleted = false)
    {
        Title = title;
        Description = description;
        Timestamp = timestamp;
        HideTimeInTimestamp = hideTimeInTimestamp;
        Icon = icon;
        Error = error;
        IsCompleted = isCompleted;
    }

    #region Method-chaining
    /// <summary>
    /// Mark this step with the given error.
    /// </summary>
    public TimelineStep SetError(string error)
    {
        Error = error;
        return this;
    }

    /// <summary>
    /// Set timestamp of the step. Also marks the step as completed unless markAsComplete is set to false.
    /// </summary>
    public TimelineStep SetTimestamp(DateTimeOffset dateTime, bool hideTime = false, bool markAsComplete = true)
    {
        Timestamp = dateTime;
        HideTimeInTimestamp = hideTime;
        IsCompleted = markAsComplete;
        return this;
    }

    /// <summary>
    /// Mark this step with the given icon.
    /// <para>Find one at https://material.io/resources/icons/?style=baseline</para>
    /// </summary>
    public TimelineStep SetIcon(string iconName)
    {
        Icon = iconName;
        return this;
    }

    /// <summary>
    /// Add the given description to this step.
    /// </summary>
    public TimelineStep SetDescription(string description)
    {
        Description = description;
        return this;
    }

    /// <summary>
    /// Add a link to the dialog for the step.
    /// </summary>
    public TimelineStep AddUrl(string url, string title = null)
    {
        Links.Add(new HyperLink(url, title ?? url));
        return this;
    }

    /// <summary>
    /// Add a set of links to the dialog for the step.
    /// </summary>
    public TimelineStep AddUrls(IEnumerable<string> urls)
    {
        foreach (var url in urls)
        {
            AddUrl(url);
        }
        return this;
    }

    /// <summary>
    /// Add a set of links to the dialog for the step.
    /// </summary>
    public TimelineStep AddUrls(IEnumerable<HyperLink> links)
    {
        Links.AddRange(links);
        return this;
    }

    /// <summary>
    /// Mark the step as completed.
    /// </summary>
    public TimelineStep SetCompleted(bool completed = true)
    {
        IsCompleted = completed;
        return this;
    }
    #endregion
}
