using HealthCheck.Core.Modules.Dataflow;
using HealthCheck.WebUI.Services;
using System;
using System.Collections.Generic;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestEntry : IDataflowEntryWithInsertionTime
    {
        public DateTime? InsertionTime { get; set; }

        public string Icon => "<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"24\" viewBox=\"0 0 24 24\" width=\"24\"><path d=\"M9 11.75c-.69 0-1.25.56-1.25 1.25s.56 1.25 1.25 1.25 1.25-.56 1.25-1.25-.56-1.25-1.25-1.25zm6 0c-.69 0-1.25.56-1.25 1.25s.56 1.25 1.25 1.25 1.25-.56 1.25-1.25-.56-1.25-1.25-1.25zM12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8 0-.29.02-.58.05-.86 2.36-1.05 4.23-2.98 5.21-5.37C11.07 8.33 14.05 10 17.42 10c.78 0 1.53-.09 2.25-.26.21.71.33 1.47.33 2.26 0 4.41-3.59 8-8 8z\"/><path d=\"M0 0h24v24H0z\" fill=\"none\"/></svg>";
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
