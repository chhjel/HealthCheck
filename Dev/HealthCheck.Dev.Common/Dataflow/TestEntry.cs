using HealthCheck.Core.Modules.Dataflow.Abstractions;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Dev.Common.Dataflow
{
    public class TestEntry : IDataflowEntryWithInsertionTime
    {
        public DateTimeOffset? InsertionTime { get; set; }

        public string Icon =>
            (InsertionTime == null) ? MaterialIcons.AllIcons.Error :
                (InsertionTime.Value.Ticks % 2 == 0)
                ? MaterialIcons.Grouped.Navigation.Check
                : MaterialIcons.Grouped.Navigation.Close;
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

        public List<string> TestList => new List<string>() { "EntryPointNotFoundException", "DuplicateWaitObjectException", "ExecutionEngineException" };
        public string TestLink => "https://www.google.com";
        public string TestImage => "https://previews.123rf.com/images/victoroancea/victoroancea1201/victoroancea120100059/12055848-tv-color-test-pattern-test-card-for-pal-and-ntsc.jpg";
    }
}
