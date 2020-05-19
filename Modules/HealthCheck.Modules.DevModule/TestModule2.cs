using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Modules.DevModule
{
    public class TestModule2 : HealthCheckModuleBase<TestModule2.TestModule2AccessOption>
    {
        public override object GetFrontendOptionsObject() => new { PropX = "asdasd asd", PropY = 135135, Now = DateTime.Now };
        public override IHealthCheckModuleConfig GetModuleConfig() => new TestModuleConfig2();

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberOne)]
        public void TestSimple() { }

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberOne)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberOne)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberOne)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberTwo)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberTwo | TestModule2AccessOption.NumberOne)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberTwo)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleAccess(TestModule2AccessOption.NumberTwo)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModule2AccessOption
        {
            NumberOne = 1,
            NumberTwo = 2
        }
    }
}
