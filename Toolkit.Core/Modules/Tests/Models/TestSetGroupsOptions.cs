using System.Collections.Generic;
using System.Linq;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

/// <summary>
/// Options for test set groups.
/// </summary>
public class TestSetGroupsOptions
{
    private readonly Dictionary<string, TestSetGroupOptions> Options = new();

    /// <summary>
    /// Set a groups option by group name.
    /// </summary>
    /// <param name="groupName">Target group</param>
    /// <param name="uiOrder">Order in the list. Higher value = higher up. Default is 0 for named groups and -1 for the 'Other' group.</param>
    public TestSetGroupsOptions ConfigureGroup(string groupName, int uiOrder)
    {
        var entry = Options.ContainsKey(groupName) ? Options[groupName] : new TestSetGroupOptions();
        Options[groupName] = entry;

        entry.GroupName = groupName;
        entry.UIOrder = uiOrder;

        return this;
    }

    /// <summary>
    /// Get a list of all defined options.
    /// </summary>
    public List<TestSetGroupOptions> GetOptions()
    {
        return Options.Keys.Select(x => Options[x]).ToList();
    }
}
