using QoDL.Toolkit.Core.Attributes;
using QoDL.Toolkit.Module.DataExport.Abstractions;
using QoDL.Toolkit.Module.DataExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static QoDL.Toolkit.Dev.Common.DataExport.TestDataExportStreamEnumerableWithQueryAndCustomParameters;

namespace QoDL.Toolkit.Dev.Common.DataExport
{
    public class TestDataExportStreamEnumerableWithQueryAndCustomParameters : TKDataExportStreamBase<TestExportItem, Parameters>
    {
        public override string StreamDisplayName => "Enumerable stream with query and custom parameters";
        public override string StreamDescription => "A test for use during dev.";
        //public override string StreamGroupName => null;
        //public override object AllowedAccessRoles => null;
        public override List<string> Categories => new() { "Test category here" };
        public override int ExportBatchSize => 50000;
        public override bool SupportsQuery() => true;

        protected override Task<TypedEnumerableResult> GetEnumerableItemsAsync(TKDataExportFilterDataTyped<TestExportItem, Parameters> filter)
        {
            var matches = Enumerable.Range(1, 1000000)
                .Select(x => new TestExportItem
                {
                    Id = $"#{x}",
                    Name = $"Item {x * 100}\nWith some newlines.\r\nAnd\tanother\n\rone\retc.",
                    Value = x,
                    StringList = new List<string> { "A1", "B2", "C3" },
                    ComplexCollection = new ComplexCollection()
                    {
                        new SimpleObj(1, "Item #1"),
                        new SimpleObj(2, "Item #2"),
                        new SimpleObj(3, "Item #3"),
                        new SimpleObj(4, "Item #4")
                    },
                    Forms = new List<TestExportItemForm>
                    {
                        new TestExportItemForm
                        {
                            FormId = "f1",
                            FormName = "The one form",
                            FormValue = 1111,
                            Values = new List<string> { "A", "B", "C"},
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
                            Shipments = null,
                            Values = new List<string> { "aa", "bb", "cc"},
                        }
                    },
                    EnumerableTest = Enumerable.Range(0, 10).Select(x => new SimpleObj(x, $"Enumerable #{x}")).ToList(),
                    EnumerableTTest = Enumerable.Range(0, 10).Select(x => new SimpleObj(x, $"Enumerable T #{x}")).ToList(),
                    CollectionTest = Enumerable.Range(0, 10).Select(x => new SimpleObj(x, $"Collection #{x}")).ToList(),
                    CollectionTTest = Enumerable.Range(0, 10).Select(x => new SimpleObj(x, $"Collection T #{x}")).ToList()
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

        public class SimpleObj
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public SimpleObj(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        public class ComplexCollection : ComplexCollectionBase<SimpleObj>
        {

        }

        public class ComplexCollectionBase<T> : List<T>, IEnumerable<T>
        {

        }

        public class TestExportItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Value { get; set; }
            public ComplexCollection ComplexCollection { get; set; }
            public List<string> StringList { get; set; }
            public ICollection<TestExportItemForm> Forms { get; set; }
            public System.Collections.ICollection CollectionTest { get; set; }
            public ICollection<SimpleObj> CollectionTTest { get; set; }
            public System.Collections.IEnumerable EnumerableTest { get; set; }
            public IEnumerable<SimpleObj> EnumerableTTest { get; set; }
        }
        public class TestExportItemForm
        {
            public string FormId { get; set; }
            public string FormName { get; set; }
            public int FormValue { get; set; }
            public List<string> Values { get; set; }
            public ICollection<TestExportItemShipment> Shipments { get; set; }
        }
        public class TestExportItemShipment
        {
            public string ShipmentId { get; set; }
            public string StreetName { get; set; }
            public int StreetNumber { get; set; }
        }

        public class Parameters
        {
            [TKCustomProperty(NullName = "Oldest")]
            public DateTime? From { get; set; } = DateTime.Now.AddDays(-30);

            [TKCustomProperty(NullName = "Newest")]
            public DateTime? To { get; set; }

            [TKCustomProperty(Description = "Limit max results in order to gain some performance.")]
            public int MaxResults { get; set; } = 1000;
        }
    }
}
