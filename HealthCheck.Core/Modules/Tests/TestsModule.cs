using HealthCheck.Core.Abstractions.Modules;
using System;

namespace HealthCheck.Core.Modules.Tests
{
    /// <summary>
    /// Module for executing tests at runtime.
    /// </summary>
    public class TestsModule : HealthCheckModuleBase<TestsModule.TestsModuleAccessOption>
    {
        /// <summary>
        /// Get frontend options for this module.
        /// </summary>
        public override object GetFrontendOptionsObject() => null;

        /// <summary>
        /// Get config for this module.
        /// </summary>
        public override IHealthCheckModuleConfig GetModuleConfig() => new TestsModuleConfig();

        /// <summary>
        /// Different access options for this module.
        /// </summary>
        [Flags]
        public enum TestsModuleAccessOption
        {
            /// <summary>
            /// Does nothing except giving access to view the module.
            /// </summary>
            None = 0

            //EditThing = 1,
            //DeleteThing = 2
        }

        //[HealthCheckModuleAccess(TestModuleAccessOption.DeleteThing)]
        //public async Task TestSimpleAsync() => await Task.Delay(10);
    }
}
