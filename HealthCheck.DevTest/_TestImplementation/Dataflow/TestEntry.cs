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

        public Dictionary<string, string> Properties { get; set; }
    }
}
