﻿using HealthCheck.Core.Util.Collections;
using HealthCheck.Module.DataExport.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamHeavy : HCDataExportStreamBase<TestDataExportStreamHeavy.HeavyItem>
    {
        public override string StreamDisplayName => "Heavy prop stream";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new List<string> { "Test category here" };
        public override int ExportBatchSize => 50000;
        public override int? MaxMemberDiscoveryDepth => 1000;
        public override IHCDataExportStream.QueryMethod Method => IHCDataExportStream.QueryMethod.Enumerable;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(int pageIndex, int pageSize, Func<HeavyItem, bool> predicate)
        {
            var matches = Enumerable.Range(1, 1000)
                .Select(x => new HeavyItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x
                })
                .Where(predicate);

            var pageItems = matches
                .Skip(pageIndex * pageSize)
                .Take(pageSize);

            return Task.FromResult(new TypedEnumerableResult
            {
                PageItems = pageItems,
                TotalCount = matches.Count()
            });
        }

        public class HeavyItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public object Obj { get; set; }
            public Array Arr { get; set; }
            public Semaphore Sema { get; set; }
            public HCDelayedBufferQueue<string> DBQ { get; set; }
            public RecursiveSubItem RecursiveSub { get; set; }
        }
        public class RecursiveSubItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public RecursiveSubItem Parent { get;  set; }
        }
    }
}