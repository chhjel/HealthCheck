using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using QoDL.Toolkit.Core.Util;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Dev.Common.Dataflow;

public class TestEntry : IDataflowEntryWithInsertionTime
{
    public DateTimeOffset? InsertionTime { get; set; }

    public string Icon =>
        (InsertionTime == null) ? TKMaterialIcons.AllIcons.Error :
            (InsertionTime.Value.Ticks % 2 == 0)
            ? TKMaterialIcons.Grouped.Navigation.Check
            : TKMaterialIcons.Grouped.Navigation.Close;
    public string Code { get; set; }
    public string Name { get; set; }
    public string PreformattedTest => "\tSomething\n\t\there!";
    public string HtmlTest => "<i>something</i> html <a href=\"#\">here</a>!";
    public DateTimeOffset TestDate { get; set; } = DateTime.Now;
    public DateTimeOffset TestDateOffset { get; set; } = DateTimeOffset.Now;

    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>
    {
        { "KeyA", "Value A" },
        { "KeyB", "Value B" },
        { "KeyC", "Value C" },
    };

    public List<string> TestList => new() { "EntryPointNotFoundException", "DuplicateWaitObjectException", "ExecutionEngineException" };
    public string TestLink => "https://www.google.com";
    public string TestImage => "https://previews.123rf.com/images/victoroancea/victoroancea1201/victoroancea120100059/12055848-tv-color-test-pattern-test-card-for-pal-and-ntsc.jpg";
}
