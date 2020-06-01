using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleA : HealthCheckModuleBase<TestModuleA.TestModuleAAccessOption>
    {
        public override object GetFrontendOptionsObject(HealthCheckModuleContext context)
            => new { PropA = "something", PropB = 123, AccessInput = context.CurrentRequestModuleAccessOptions.ToString() };
        public override IHealthCheckModuleConfig GetModuleConfig(HealthCheckModuleContext context)
            => new TestModuleAConfig();

        [HealthCheckModuleMethod(TestModuleAAccessOption.EditThing)]
        public void TestSimple() { }

        [HealthCheckModuleMethod(TestModuleAAccessOption.EditThing)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleMethod(TestModuleAAccessOption.EditThing)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleMethod(TestModuleAAccessOption.EditThing)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleMethod(TestModuleAAccessOption.DeleteThing | TestModuleAAccessOption.EditThing)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModuleAAccessOption
        {
            EditThing = 1,
            DeleteThing = 2
        }
    }
}
