using HealthCheck.WebUI.Services;
using System;

namespace HealthCheck.DevTest._TestImplementation.Dataflow
{
    public class TestEntry : IDataflowEntryWithInsertionTime
    {
        public DateTime? InsertionTime { get; set; }

        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
