using HealthCheck.Core.Modules.EventNotifications.Abstractions;
using HealthCheck.Core.Modules.Settings.Abstractions;
using HealthCheck.Core.Modules.SiteEvents.Utils;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Dev.Common.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests.Modules
{
    [RuntimeTestClass(
        Name = "Event Notifications",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.Modules,
        UIOrder = 30
    )]
    public class EventNotificationTests
    {
        private readonly IEventDataSink _eventDataSink;
        private readonly IHCSettingsService _settingsService;

        public EventNotificationTests(IEventDataSink eventDataSink, IHCSettingsService settingsService)
        {
            _eventDataSink = eventDataSink;
            _settingsService = settingsService;
        }

        public TestResult TestParallel(int count = 50, string id = "event_parallel_test")
        {
            Task.Run(() =>
            {
                Parallel.ForEach(Enumerable.Range(1, count), (i) =>
                {
                    _eventDataSink.RegisterEvent(id, new
                    {
                        Number = i,
                        TimeStamp = DateTimeOffset.Now,
                        RandomValue = new Random().Next(1000),
                        Guid = Guid.NewGuid()
                    });
                });
            });
            return TestResult.CreateSuccess($"Registered #{count} events parallel.");
        }

        [RuntimeTest]
        public TestResult RegisterEvent(int variant)
        {
            object payload = variant switch
            {
                3 => new
                {
                    Url = "https://localhost:8888/test3",
                    User = "DevUser",
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp,
                    ExtraB = "BBBB"
                },
                2 => new
                {
                    Url = "https://localhost:8888/test2",
                    User = "DevUser",
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp,
                    ExtraA = "AAAA"
                },
                _ => new
                {
                    Url = "https://localhost:8888/testX",
                    User = "DevUser",
                    SettingValue = _settingsService.GetSettings<TestSettings>().IntProp
                },
            };
            _eventDataSink.RegisterEvent("pageload", payload);
            return TestResult.CreateSuccess($"Registered variant #{variant}");
        }

        [RuntimeTest]
        public TestResult SimulateSiteEventResolveJob()
        {
            var count = 0;
            var resolved = 0;
            var unresolvedEvents = HCSiteEventUtils.TryGetAllUnresolvedEvents();
            foreach (var unresolvedEvent in unresolvedEvents)
            {
                count++;
                var timeSince = DateTimeOffset.Now - (unresolvedEvent.Timestamp + TimeSpan.FromMinutes(unresolvedEvent.Duration));
                if (timeSince > TimeSpan.FromMinutes(15))
                {
                    resolved++;
                    HCSiteEventUtils.TryMarkEventAsResolved(unresolvedEvent.Id, "Seems to be fixed now.");
                }
            }
            return TestResult.CreateSuccess($"Job simulated {count} unresolved events, {resolved} attempted resolved.");
        }
    }
}
