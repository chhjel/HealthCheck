using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Module.DevModule
{
    /// <summary>
    /// A broken module for testing.
    /// </summary>
    public class TestModuleB : HealthCheckModuleBase<TestModuleB.TestModuleBAccessOption>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context) 
            => new { PropX = "asdasd asd", PropY = 135135, Now = DateTime.Now, AccessInput = context.CurrentRequestModuleAccessOptions.ToString() };
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context) => null;

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestSimple()
        {
            // Method intentionally left empty.
        }

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestSimple(int id) { Console.WriteLine(id); }

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberOne)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberOne)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberTwo | TestModuleBAccessOption.NumberOne)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        public enum TestModuleBAccessOption
        {
            NumberOne = 1,
            NumberTwo = 2
        }
    }
}
