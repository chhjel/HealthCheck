using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestEntry : IDataflowEntryWithInsertionTime
    {
        public DateTime? InsertionTime { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public string PreformattedTest => "\tSomething\n\t\there!";
        public string HtmlTest => "<i>something</i> html <a href=\"#\">here</a>!";

        public Dictionary<string, string> Properties { get; set; }

        public List<string> TestList => new List<string>() { "EntryPointNotFoundException", "DuplicateWaitObjectException", "ExecutionEngineException" };
        public string TestLink => "https://www.google.com";
        public string TestImage => "https://previews.123rf.com/images/victoroancea/victoroancea1201/victoroancea120100059/12055848-tv-color-test-pattern-test-card-for-pal-and-ntsc.jpg";
    }
}
