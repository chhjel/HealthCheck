﻿using HealthCheck.Core.Attributes;
using HealthCheck.Module.DataExport.Abstractions;
using HealthCheck.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static HealthCheck.Dev.Common.DataExport.TestDataExportStreamEnumerableWithQueryAndCustomParameters;

namespace HealthCheck.Dev.Common.DataExport
{
    public class TestDataExportStreamEnumerableWithQueryAndCustomParameters : HCDataExportStreamBase<TestExportItem, Parameters>
    {
        public override string StreamDisplayName => "Enumerable stream with query and custom parameters";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new List<string> { "Test category here" };
        public override int ExportBatchSize => 50000;
        public override bool SupportsQuery() => true;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(HCDataExportFilterDataTyped<TestExportItem, Parameters> filter)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x,
                    Forms = new List<TestExportItemForm>
                    {
                        new TestExportItemForm
                        {
                            FormId = "f1",
                            FormName = "The one form",
                            FormValue = 1111,
                            Shipments = new List<TestExportItemShipment>
                            {
                                new TestExportItemShipment
                                {
                                    ShipmentId = "s1",
                                    StreetName = "Customer street name",
                                    StreetNumber = 23
                                },
                                new TestExportItemShipment
                                {
                                    ShipmentId = "s2",
                                    StreetName = "Return street name",
                                    StreetNumber = 88
                                }
                            }
                        },
                        new TestExportItemForm
                        {
                            FormId = "f2",
                            FormName = "nope",
                            FormValue = 2222,
                            Shipments = null
                        }
                    }
                })
                .Where(filter.QueryPredicate)
                .Take(filter.Parameters.MaxResults);

            var pageItems = matches
                .Skip(filter.PageIndex * filter.PageSize)
                .Take(filter.PageSize);

            return Task.FromResult(new TypedEnumerableResult
            {
                PageItems = pageItems,
                TotalCount = matches.Count()
            });
        }

        public class TestExportItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public List<TestExportItemForm> Forms { get; set; }
        }
        public class TestExportItemForm
        {
            public string FormId { get; set; }
            public string FormName { get; set; }
            public int FormValue { get; set; }
            public List<TestExportItemShipment> Shipments { get; set; }
        }
        public class TestExportItemShipment
        {
            public string ShipmentId { get; set; }
            public string StreetName { get; set; }
            public int StreetNumber { get; set; }
        }

        public class Parameters
        {
            [HCCustomProperty(NullName = "Oldest")]
            public DateTime? From { get; set; } = DateTime.Now.AddDays(-30);

            [HCCustomProperty(NullName = "Newest")]
            public DateTime? To { get; set; }

            [HCCustomProperty(Description = "Limit max results in order to gain some performance.")]
            public int MaxResults { get; set; } = 1000;
        }
    }
}
