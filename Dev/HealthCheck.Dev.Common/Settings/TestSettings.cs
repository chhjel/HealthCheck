using HealthCheck.Core.Modules.Settings.Attributes;
using System;

namespace HealthCheck.Dev.Common.Settings
{
    public class TestSettings
    {
        [HCSetting(Description = "Some description here")]
        public string StringProp { get; set; }
        public bool BoolProp { get; set; } = false;
        public int IntProp { get; set; } = 15523;

        [HCSetting(GroupName = "Store latest mail/sms")]
        public bool EnableStoringMessages { get; set; } = true;

        [HCSetting(GroupName = "Service X")]
        public bool EnableX { get; set; }

        [HCSetting(GroupName = "Service X")]
        public string ConnectionString { get; set; } = "This is default";

        [HCSetting(GroupName = "Service X")]
        public int Threads { get; set; } = 2;

        [HCSetting(GroupName = "Service X")]
        public int NumberOfThings { get; set; } = 321;

        [HCSetting(GroupName = "Event Notifications")]
        public bool EnableEventRegistering { get; set; }

        [HCSetting(GroupName = "Event Notifications")]
        public DateTime StartAt { get; set; } = DateTime.Now.AddDays(-5);
    }
}
