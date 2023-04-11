using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Abstractions;
using QoDL.Toolkit.Core.Modules.SiteEvents.Enums;
using QoDL.Toolkit.Core.Modules.SiteEvents.Models;
using QoDL.Toolkit.Core.Modules.SiteEvents.Utils;
using QoDL.Toolkit.Core.Modules.Tests.Attributes;
using QoDL.Toolkit.Core.Modules.Tests.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Dev.Common.Tests.Modules
{
    [RuntimeTestClass(
        Name = "Site Events",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
        GroupName = RuntimeTestConstants.Group.Modules,
        UIOrder = 30
    )]
    public class SiteEventTests
    {
        private static readonly Random _rand = new();
        private static readonly DateTime _eventTime = DateTime.Now;
        private readonly ISiteEventService _siteEventService;

        public SiteEventTests(ISiteEventService siteEventService)
        {
            _siteEventService = siteEventService;
        }

        [RuntimeTest]
        public async Task<TestResult> AddEvents(int count)
        {
            for (int i = 0; i < count; i++)
            {
                await AddEvent();
            }
            return TestResult.CreateSuccess("Some events should be created now.");
        }

        [RuntimeTest]
        public TestResult AddEvent(SiteEventSeverity severity, string eventTypeId, string title, string description,
            int duration = 1, string developerDetails = null)
        {
            TKSiteEventUtils.TryRegisterNewEvent(severity, eventTypeId, title, description, duration, developerDetails,
                config: x => x.AddRelatedLink("Status page", "https://status.otherapi.com"));
            return TestResult.CreateSuccess("Event should be created now.");
        }

        [RuntimeTest]
        public TestResult AddEventsPreset_Misc1(int count = 500, SiteEventSeverity severity = SiteEventSeverity.Warning)
        {
            for (int i = 0; i < count; i++)
            {
                TKSiteEventUtils.TryRegisterNewEvent(severity, $"pageError_{_eventTime.Ticks}", $"Slow page #{_eventTime.Ticks}",
                    $"Pageload seems a bit slow currently on page #{_eventTime.Ticks}, we're working on it.",
                    duration: 5,
                    developerDetails: $"Duration: {i} ms");
            }
            return TestResult.CreateSuccess($"{count} events should be created now.");
        }

        [RuntimeTest]
        public TestResult AddEventsPreset_Severities1()
        {
            TKSiteEventUtils.TryRegisterNewEvent(new SiteEvent(SiteEventSeverity.Information, "test_info", "Some info here", "Some more details etc.",
                developerDetails: "Hmm this is probably why.")
            { Timestamp = DateTimeOffset.Now - TimeSpan.FromMinutes(5), Duration = 5 });
            TKSiteEventUtils.TryRegisterNewEvent(new SiteEvent(SiteEventSeverity.Warning, "test_warn", "Some warning here", "Some more details etc.",
                developerDetails: "Hmm this is probably why.")
            { Timestamp = DateTimeOffset.Now - TimeSpan.FromMinutes(500), Duration = 450 });
            TKSiteEventUtils.TryRegisterNewEvent(new SiteEvent(SiteEventSeverity.Error, "test_error", "Some error here", "Some more details etc.",
                developerDetails: "Hmm this is probably why.")
            { Timestamp = DateTimeOffset.Now - TimeSpan.FromMinutes(60), Duration = 60 });
            TKSiteEventUtils.TryRegisterNewEvent(new SiteEvent(SiteEventSeverity.Fatal, "test_fatal", "Some fatal things here", "Some more details etc.",
                developerDetails: "Hmm this is probably why.")
            { Timestamp = DateTimeOffset.Now - TimeSpan.FromMinutes(36), Duration = 30 });
            return TestResult.CreateSuccess("Some events should be created now.");
        }

        [RuntimeTest]
        public TestResult AddEventsPreset_TimeCheck1()
        {
            var now = DateTime.Now;
            var times = new List<(string, DateTime)>
                {
                    ("aaa", new DateTime(now.Year, now.Month, now.Day - 2, 15, 38, 05)),
                    ("ccc", new DateTime(now.Year, now.Month, now.Day - 1, 8, 11, 05)),
                    ("ddd", new DateTime(now.Year, now.Month, now.Day - 1, 6, 09, 05))
                };
            var from = new DateTime(now.Year, now.Month, now.Day - 2, 12, 23, 05);
            var to = new DateTime(now.Year, now.Month, now.Day - 1, 0, 12, 52);
            for (var d = from; d <= to; d += TimeSpan.FromMinutes(1))
            {
                times.Add(("bbb", d));
            }

            times.AddRange(new List<(string, DateTime)>
                {
                    ("aaa", new DateTime(now.Year, now.Month, now.Day - 2, 15, 38, 05)),
                    ("ccc", new DateTime(now.Year, now.Month, now.Day - 1, 8, 11, 05)),
                    ("ddd", new DateTime(now.Year, now.Month, now.Day - 1, 6, 09, 05))
                });

            foreach (var d in times)
            {
                var e = new SiteEvent(SiteEventSeverity.Error, $"test_{d.Item1}", $"Oh no! API {d.Item1.ToUpper()} is broken!", "How could this happen to us!?",
                    developerDetails: "Hmm this is probably why.")
                {
                    Timestamp = d.Item2
                };
                TKSiteEventUtils.TryRegisterNewEvent(e);
            }
            return TestResult.CreateSuccess("Some events should be created now.");
        }

        [RuntimeTest]
        public TestResult TryMarkLatestEventAsResolved(string eventTypeId = "api_x_error")
        {
            TKSiteEventUtils.TryMarkLatestEventAsResolved("api_x_error", "Seems it fixed itself somehow.",
                config: x => x.AddRelatedLink("Another page", "https://www.google.com"));
            return TestResult.CreateSuccess("Hopefully worked.");
        }

        public async Task AddEvent()
        {
            CreateSomeData(out string title, out string description);
            var severity = SiteEventSeverity.Information;
            if (_rand.Next(100) < 10)
            {
                severity = SiteEventSeverity.Fatal;
            }
            else if (_rand.Next(100) < 25)
            {
                severity = SiteEventSeverity.Error;
            }
            else if (_rand.Next(100) < 50)
            {
                severity = SiteEventSeverity.Warning;
            }

            var ev = new SiteEvent(
                severity, $"Error type {_rand.Next(10000)}",
                title, description,
                duration: _rand.Next(1, 90)
            )
            {
                Timestamp = DateTimeOffset.Now
                    .AddDays(-7 + _rand.Next(7))
                    .AddMinutes(_rand.Next(0, 24 * 60))
            }
            .AddRelatedLink("Page that failed", "https://www.google.com?etc")
            .AddRelatedLink("Error log", "https://www.google.com?q=errorlog");

            await _siteEventService.StoreEvent(ev);
        }

        private void CreateSomeData(out string title, out string description)
        {
            var subject = _subjects.RandomElement(_rand);
            subject = AddXFix(subject, _subjectXFixes.RandomElement(_rand));
            var accident = _accidents.RandomElement(_rand);
            var reaction = _reactions.RandomElement(_rand);
            var reactor = _subjects.RandomElement(_rand);
            reactor = AddXFix(reactor, _subjectXFixes.RandomElement(_rand));

            title = $"{subject} {accident}".CapitalizeFirst();
            description = $"{subject} {accident} and {reactor} is {reaction}.".CapitalizeFirst();
        }

        private static string AddXFix(string subject, string xfix)
        {
            if (xfix.Contains("|"))
            {
                var parts = xfix.Split('|');
                var prefix = parts[0];
                var suffix = parts[1];
                return $"{prefix} {subject}{suffix}";
            }
            else
            {
                return $"{xfix}{subject}";
            }
        }

        private readonly string[] _subjectXFixes = new[] { "the ", "an unknown ", "most of the |s", "several of the |s", "one of the |s" };
        private readonly string[] _subjects = new[] { "service", "server", "integration", "frontpage", "developer", "codebase", "project manager", "CEO" };
        private readonly string[] _accidents = new[] { "is on fire", "exploded", "is slow", "decided to close", "is infected with ransomware", "is not happy", "don't know what to do" };
        private readonly string[] _reactions = new[] { "on fire", "not pleased", "confused", "not happy", "leaving" };
    }
}
