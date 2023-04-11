using QoDL.Toolkit.Core.Modules.SiteEvents.Enums;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.SiteEvents.Services;
using QoDL.Toolkit.Episerver.Storage;
using QoDL.Toolkit.Episerver.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace QoDL.Toolkit.Episerver.Tests.Storage
{
    public class TKEpiserverBlobSiteEventStorageTests
    {
        [Fact]
        public async Task StoreEvent_WithMaxCount_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(50)
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_" + id, Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                tasks.Add(service.StoreEvent(createEvent(i)));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task StoreEvent_WithMaxAge_RespectsIt()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemAge(TimeSpan.FromDays(1))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_" + id, Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var e = createEvent(i);
                if (i % 2 == 0)
                {
                    e.Timestamp = e.Timestamp.AddDays(-10);
                }
                tasks.Add(service.StoreEvent(e));
            }
            await Task.WhenAll(tasks);
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(50, items.Count);
        }

        [Fact]
        public async Task StoreEvent_ToUpdateUsingService_ShouldNotCreateNew()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id)
                => new SiteEvent { EventTypeId = "type_x", Title = $"Title#{id}", Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                tasks.Add(service.StoreEvent(createEvent(i)));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.True(items.TrueForAll(x => x.Id != Guid.Empty), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
            Assert.Single(items);

            storage.ForceBufferCallback();
            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
        }

        [Fact]
        public async Task StoreEvent_ToUpdateUsingService_UpdatesCorrectItem()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = DateTimeOffset.Now, Duration = 1 };

            var service = new SiteEventService(storage);

            var tasks = new List<Task>
            {
                service.StoreEvent(createEvent(0, "A")),
                service.StoreEvent(createEvent(1, "B"))
            };
            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "A";
            items[1].Title = "B";

            storage.ForceBufferCallback();
            tasks.Add(service.StoreEvent(createEvent(0, "A")));
            tasks.Add(service.StoreEvent(createEvent(1, "B")));

            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "Av2";
            items[1].Title = "Bv2";

            storage.ForceBufferCallback();

            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            items[0].Title = "Av2";
            items[1].Title = "Bv2";
        }

        [Fact]
        public async Task StoreEvent_WithUpdateOutsideMergeWindow_RespectsMergeThreshold()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name, DateTimeOffset timestamp)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = timestamp, Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(createEvent(1, "Event Old", DateTimeOffset.Now.AddMinutes(-(service.DefaultMergeOptions.MaxMinutesSinceLastEventEnd+2))));
            storage.ForceBufferCallback();

            await service.StoreEvent(createEvent(1, "Event New", DateTimeOffset.Now));
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            Assert.Equal("Event Old", items[0].Title);
            Assert.Equal("Event New", items[1].Title);
        }

        [Fact]
        public async Task StoreEvent_WithUpdateWithinMergeWindow_RespectsMergeThreshold()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent createEvent(int id, string name, DateTimeOffset timestamp)
                => new SiteEvent { EventTypeId = "type_" + id, Title = name, Description = $"Desc#{id}", Timestamp = timestamp, Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(createEvent(1, "Event Old", DateTimeOffset.Now - TimeSpan.FromMinutes(service.DefaultMergeOptions.MaxMinutesSinceLastEventEnd / 2)));
            storage.ForceBufferCallback();

            await service.StoreEvent(createEvent(1, "Event New", DateTimeOffset.Now));
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
            Assert.Equal("Event New", items[0].Title);
        }

        [Fact]
        public async Task StoreEvent_WhenBuffered_ItemsShouldHaveUniqueIds()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromSeconds(5);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var eventX = new SiteEvent
                {
                    EventTypeId = "typeId_" + i,
                    Title = $"Title#{i}",
                    Description = $"Desc#{i}",
                };
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.True(items.TrueForAll(x => x.Id != Guid.Empty), "All items should have generated ids");
            Assert.True(items.GroupBy(x => x.Id).All(x => x.Count() == 1), "All items should have unique ids");
        }

        [Fact]
        public async Task StoreEvent_WithLessThanMax_ShouldBeAvailableAtOnce()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++)
            {
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeId_" + i, "TitleB", "DescriptionB");
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(100, items.Count); // Should be 100 items stored
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(0, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        [Fact]
        public async Task StoreEvent_WithMaxCount_ShouldNotOverflow()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            storage.MaxBufferSize = 500;

            var tasks = new List<Task>();
            for (int i = 0; i < 5000; i++)
            {
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeId_"+i, "TitleB", "DescriptionB");
                tasks.Add(storage.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(1000, items.Count); // Should be 1000 items stored
            Assert.Equal(1, storage.LoadCounter); // Should get only once
            Assert.Equal(10, storage.SaveCounter); // Store count should be inserts / max buffer size = 0
        }

        [Fact]
        public async Task StoreEvent_WithManyMergedEvents_ShouldStoreSingle()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob);
            var service = new SiteEventService(storage);
            var tasks = new List<Task>();
            for (int i = 0; i < 5000; i++)
            {
                var eventX = new SiteEvent(SiteEventSeverity.Error, "typeIdX", "TitleB", "DescriptionB");
                tasks.Add(service.StoreEvent(eventX));
            }

            await Task.WhenAll(tasks);
            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
        }

        [Fact]
        public async Task MarkLatestEventAsResolved_WithEventFromYesterday_ShouldResolveEvent()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            SiteEvent e = new SiteEvent { EventTypeId = "id", Title = "Title", Description = $"Desc", Timestamp = DateTimeOffset.Now.AddDays(-1), Duration = 1 };

            var service = new SiteEventService(storage);

            await service.StoreEvent(e);
            storage.ForceBufferCallback();

            await service.MarkLatestEventAsResolved(e.EventTypeId, "Resolved!");
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Single(items);
            Assert.Equal(e.Id, items[0].Id);
            Assert.Equal(e.EventTypeId, items[0].EventTypeId);
            Assert.Equal("Resolved!", items[0].ResolvedMessage);
            Assert.True(items[0].Resolved);
        }

        [Fact]
        public async Task MarkLatestEventAsResolved_WithEventFromYesterdayAndResolvedEarlier_ShouldResolveEvent()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob)
                .SetMaxItemCount(500)
                .SetMaxItemAge(TimeSpan.FromDays(30))
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var service = new SiteEventService(storage);

            var earlyEvent = new SiteEvent
            {
                EventTypeId = "id1",
                Title = "Title1",
                Description = $"Desc1",
                Timestamp = DateTimeOffset.Now.AddDays(-2),
                Duration = 1,
                Resolved = true,
                ResolvedAt = DateTimeOffset.Now.AddDays(-1.5),
                ResolvedMessage = "Resolved1"
            };
            await service.StoreEvent(earlyEvent);
            storage.ForceBufferCallback();

            var e = new SiteEvent { EventTypeId = "id2", Title = "Title2", Description = $"Desc2", Timestamp = DateTimeOffset.Now.AddDays(-1), Duration = 1 };
            await service.StoreEvent(e);
            storage.ForceBufferCallback();

            await service.MarkLatestEventAsResolved(e.EventTypeId, "Resolved2");
            storage.ForceBufferCallback();

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(2, items.Count);
            Assert.Equal(earlyEvent.Id, items[0].Id);
            Assert.Equal(e.Id, items[1].Id);
            Assert.Equal("id1", items[0].EventTypeId);
            Assert.Equal("id2", items[1].EventTypeId);
            Assert.Equal("Resolved1", items[0].ResolvedMessage);
            Assert.Equal("Resolved2", items[1].ResolvedMessage);
            Assert.True(items[0].Resolved);
            Assert.True(items[1].Resolved);
        }

        [Fact]
        public async Task MarkEventAsResolved_UsingGetUnresolvedEvents_ShouldResolveEvents()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), JsonScenario1);
            var storage = CreateStorage(() => blob).SetMaxItemCount(500)
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            var service = new SiteEventService(storage);

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(13, items.Count);

            var unresolvedItems = await service.GetUnresolvedEvents();
            Assert.Equal(8, unresolvedItems.Count);

            foreach (var e in unresolvedItems)
            {
                await service.MarkEventAsResolved(e.Id, "The issue seems to be resolved now.");
            }

            items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(13, items.Count);
            Assert.Equal(13, items.Count(x => x.Resolved));
            Assert.Equal(13, items.Count(x => x.ResolvedAt != null));
            Assert.Equal(13, items.Count(x => !string.IsNullOrWhiteSpace(x.ResolvedMessage)));

            unresolvedItems = await service.GetUnresolvedEvents();
            Assert.Empty(unresolvedItems);
        }

        [Fact]
        public async Task GetLast_WithLatestFirst_GetsLatest()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob).SetMaxItemCount(500)
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 0; i < 10; i++)
            {
                var e = new SiteEvent { EventTypeId = "etid", Title = "Title" + i, Description = "Desc" + i, Timestamp = DateTimeOffset.Now.AddDays(-i), Duration = 1, Resolved = true };
                await storage.StoreEvent(e);

                e = new SiteEvent
                {
                    EventTypeId = "etidX",
                    Title = "TitleX" + i,
                    Description = "DescX" + i,
                    Timestamp = DateTimeOffset.Now.AddDays(-i).AddMinutes(1),
                    Duration = 1
                };
                await storage.StoreEvent(e);

            }

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(20, items.Count);

            var latestMergable = await storage.GetLastMergableEventOfType("etid");
            Assert.Equal("Title0", latestMergable.Title);

            var lastUnresolved = await storage.GetLastUnresolvedEventOfType("etidX");
            Assert.Equal("TitleX0", lastUnresolved.Title);
        }

        [Fact]
        public async Task GetLast_WithLatestLast_GetsLatest()
        {
            var blob = new MockBlob(new Uri("https://mock.blob"), "{}");
            var storage = CreateStorage(() => blob).SetMaxItemCount(500)
                as TKEpiserverBlobSiteEventStorage;
            storage.MaxBufferSize = 500;
            storage.BlobUpdateBufferDuration = TimeSpan.FromDays(1);

            for (int i = 10; i > 0; i--)
            {
                var e = new SiteEvent { EventTypeId = "etid", Title = "Title" + i, Description = "Desc" + i, Timestamp = DateTimeOffset.Now.AddDays(-i), Duration = 1, Resolved = true };
                await storage.StoreEvent(e);

                e = new SiteEvent
                {
                    EventTypeId = "etidX",
                    Title = "TitleX" + i,
                    Description = "DescX" + i,
                    Timestamp = DateTimeOffset.Now.AddDays(-i).AddMinutes(1),
                    Duration = 1
                };
                await storage.StoreEvent(e);

            }

            var items = await storage.GetEvents(DateTimeOffset.MinValue, DateTimeOffset.MaxValue);
            Assert.Equal(20, items.Count);

            var latestMergable = await storage.GetLastMergableEventOfType("etid");
            Assert.Equal("Title1", latestMergable.Title);

            var lastUnresolved = await storage.GetLastUnresolvedEventOfType("etidX");
            Assert.Equal("TitleX1", lastUnresolved.Title);
        }

        private TKEpiserverBlobSiteEventStorage CreateStorage(Func<MockBlob> blobFactory = null, string blobJson = null)
        {
            var cache = EpiBlobTestHelpers.CreateMockCache();
            var factoryMock = EpiBlobTestHelpers.CreateBlobFactoryMock(blobFactory, blobJson);
            return new TKEpiserverBlobSiteEventStorage(factoryMock.Object, cache);
        }

        private const string JsonScenario1 = @"
{
    ""Items"": {
      ""250e90c0-ffe0-42d3-bd13-7ada1dbf3823"": {
        ""Id"": ""250e90c0-ffe0-42d3-bd13-7ada1dbf3823"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-10T14:11:51.0862481+01:00"",
        ""EventTypeId"": ""ServiceX_5xx"",
        ""Title"": ""ServiceX availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": true,
        ""ResolvedMessage"": ""The issue seems to be resolved now."",
        ""ResolvedAt"": ""2021-11-10T14:30:10.6050031+01:00"",
        ""AllowMerge"": true
      },
      ""a1ba86aa-13c9-4e55-a9d1-2e13686d3497"": {
        ""Id"": ""a1ba86aa-13c9-4e55-a9d1-2e13686d3497"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-11T20:52:11.5304947+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": true,
        ""ResolvedMessage"": ""The issue seems to be resolved now."",
        ""ResolvedAt"": ""2021-11-11T21:15:00.1313196+01:00"",
        ""AllowMerge"": true
      },
      ""b612eec5-aa28-4d65-8910-3398b738ec95"": {
        ""Id"": ""b612eec5-aa28-4d65-8910-3398b738ec95"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-13T02:51:41.257456+01:00"",
        ""EventTypeId"": ""ServiceX_5xx"",
        ""Title"": ""ServiceX availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""02003d21-88b1-4d6a-8609-8daa465006c0"": {
        ""Id"": ""02003d21-88b1-4d6a-8609-8daa465006c0"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-13T23:25:14.6236359+01:00"",
        ""EventTypeId"": ""ServiceZ_5xx"",
        ""Title"": ""ServiceZ availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": true,
        ""ResolvedMessage"": ""The issue seems to be resolved now."",
        ""ResolvedAt"": ""2021-11-13T23:45:00.7233975+01:00"",
        ""AllowMerge"": true
      },
      ""57e6138d-c205-4eb4-baa3-c675acdd0091"": {
        ""Id"": ""57e6138d-c205-4eb4-baa3-c675acdd0091"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-14T22:59:25.7102606+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""4f8be927-da1d-43ec-b00c-78e802d25b40"": {
        ""Id"": ""4f8be927-da1d-43ec-b00c-78e802d25b40"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-15T01:26:53.9614574+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""c85805a3-703b-4f31-9be3-903cb1e55cfc"": {
        ""Id"": ""c85805a3-703b-4f31-9be3-903cb1e55cfc"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-15T11:28:50.9349063+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""eef04d55-ee62-4870-8c04-063ac15f351e"": {
        ""Id"": ""eef04d55-ee62-4870-8c04-063ac15f351e"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-15T17:52:18.4504295+01:00"",
        ""EventTypeId"": ""ServiceX_5xx"",
        ""Title"": ""ServiceX availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""daa8cd4b-2870-40b3-98ca-149d334c605d"": {
        ""Id"": ""daa8cd4b-2870-40b3-98ca-149d334c605d"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-16T14:58:32.6470479+01:00"",
        ""EventTypeId"": ""ServiceA_5xx"",
        ""Title"": ""ServiceA availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": true,
        ""ResolvedMessage"": ""The issue seems to be resolved now."",
        ""ResolvedAt"": ""2021-11-16T15:15:00.980823+01:00"",
        ""AllowMerge"": true
      },
      ""aed53c30-05aa-480d-9595-04c6043af964"": {
        ""Id"": ""aed53c30-05aa-480d-9595-04c6043af964"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-16T15:59:43.6030209+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""f3ed401c-85fb-421d-9bb2-5d6aa2811988"": {
        ""Id"": ""f3ed401c-85fb-421d-9bb2-5d6aa2811988"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-16T18:11:50.9668919+01:00"",
        ""EventTypeId"": ""ServiceX_5xx"",
        ""Title"": ""ServiceX availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""e029c16b-14ad-4989-b9b0-fda68cb94be4"": {
        ""Id"": ""e029c16b-14ad-4989-b9b0-fda68cb94be4"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-16T20:24:10.2130044+01:00"",
        ""EventTypeId"": ""ServiceY_5xx"",
        ""Title"": ""ServiceY availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          },
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": false,
        ""ResolvedMessage"": null,
        ""ResolvedAt"": null,
        ""AllowMerge"": true
      },
      ""428b6c9a-2c69-4ce6-ba6c-3489082509be"": {
        ""Id"": ""428b6c9a-2c69-4ce6-ba6c-3489082509be"",
        ""Severity"": 1,
        ""Timestamp"": ""2021-11-17T08:25:06.1350088+01:00"",
        ""EventTypeId"": ""ServiceB_5xx"",
        ""Title"": ""ServiceB availability reduced"",
        ""Description"": ""desc"",
        ""Duration"": 1,
        ""RelatedLinks"": [
          {
            ""Text"": ""Title"",
            ""Url"": ""https://www.etc.com""
          }
        ],
        ""DeveloperDetails"": ""etc"",
        ""Resolved"": true,
        ""ResolvedMessage"": ""The issue seems to be resolved now."",
        ""ResolvedAt"": ""2021-11-17T08:45:00.1014633+01:00"",
        ""AllowMerge"": true
      }
    }
  }
";
    }
}
