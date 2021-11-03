using System;

namespace HealthCheck.Core.Tests.Storage.Implementations
{
    public class TestItem
    {
        public int Id { get; set; }
		public string Value { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
}
