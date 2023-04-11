using QoDL.Toolkit.Core.Modules.Settings.Attributes;
using System;

namespace QoDL.Toolkit.Dev.Common.Settings;

public class TestSettings
{
    [TKSetting(Description = "Some description here")]
    public string StringProp { get; set; }
    public bool BoolProp { get; set; } = false;
    public int IntProp { get; set; } = 15523;

    [TKSetting(GroupName = "Store latest mail/sms")]
    public bool EnableStoringMessages { get; set; } = true;

    [TKSetting(GroupName = "Service X")]
    public bool EnableX { get; set; }

    [TKSetting(GroupName = "Service X", UIHints = Core.Models.TKUIHint.CodeArea)]
    public string ConnectionString { get; set; } = "This is default";

    [TKSetting(GroupName = "Service X")]
    public int Threads { get; set; } = 2;

    [TKSetting(GroupName = "Service X")]
    public int NumberOfThings { get; set; } = 321;

    [TKSetting(GroupName = "Event Notifications")]
    public bool EnableEventRegistering { get; set; }

    [TKSetting(GroupName = "Event Notifications")]
    public DateTime StartAt { get; set; } = DateTime.Now.AddDays(-5);
}
