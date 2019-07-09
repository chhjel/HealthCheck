using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Models
{
    /// <summary>
    /// Options for test set groups. ToDo move to web.core
    /// </summary>
    public class TestSetGroupsOptions
    {
        private readonly Dictionary<string, TestSetGroupOptions> Options = new Dictionary<string, TestSetGroupOptions>();

        /// <summary>
        /// Set a groups option by group name.
        /// </summary>
        /// <param name="groupName">Target group</param>
        /// <param name="uiOrder">Order in the list. Higher value = higher up. Default is 0 for named groups and -1 for the 'Other' group.</param>
        public TestSetGroupsOptions SetOptionsFor(string groupName, int uiOrder)
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
}
