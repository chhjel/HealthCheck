using QoDL.Toolkit.Core.Modules.Dataflow.Abstractions;
using System;

namespace QoDL.Toolkit.Dev.Common.Dataflow
{
    public class TestMemoryStreamItem : IDataflowEntryWithInsertionTime
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTimeOffset? InsertionTime { get; set; }

        public static implicit operator TestMemoryStreamItem(string message)
            => new()
            { Message = message };
    }
}