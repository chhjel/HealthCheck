using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleB : HealthCheckModuleBase<TestModuleB.TestModuleBAccessOption>
    {
        public override object GetFrontendOptionsObject() => new { PropX = "asdasd asd", PropY = 135135, Now = DateTime.Now };
        public override IHealthCheckModuleConfig GetModuleConfig() => new TestModuleBConfig();

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberOne)]
        public void TestSimple() { }

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberOne)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberOne)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberOne)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberTwo)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberTwo | TestModuleBAccessOption.NumberOne)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberTwo)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleAccess(TestModuleBAccessOption.NumberTwo)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModuleBAccessOption
        {
            NumberOne = 1,
            NumberTwo = 2
        }
    }
}
