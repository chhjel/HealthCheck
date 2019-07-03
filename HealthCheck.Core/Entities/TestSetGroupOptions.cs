using System;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// Options for test set groups. ToDo move to web.core
    /// </summary>
    public class TestSetGroupOptions
    {
        /// <summary>
        /// Set a groups option by group name.
        /// </summary>
        public TestSetGroupOptions SetOptionsFor(string groupName, int uiOrder, string iconName)
        {
            // ToDo
            Console.WriteLine($"{groupName} | {uiOrder} | {iconName}");
            return this;
        }
    }
}
