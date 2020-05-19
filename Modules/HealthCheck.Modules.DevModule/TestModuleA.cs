using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Modules.DevModule
{
    public class TestModuleA : HealthCheckModuleBase<TestModuleA.TestModuleAAccessOption>
    {
        public override object GetFrontendOptionsObject() => new { PropA = "something", PropB = 123 };
        public override IHealthCheckModuleConfig GetModuleConfig() => new TestModuleAConfig();

        [HealthCheckModuleAccess(TestModuleAAccessOption.EditThing)]
        public void TestSimple() { }

        [HealthCheckModuleAccess(TestModuleAAccessOption.EditThing)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleAccess(TestModuleAAccessOption.EditThing)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleAccess(TestModuleAAccessOption.EditThing)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleAccess(TestModuleAAccessOption.DeleteThing)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleAccess(TestModuleAAccessOption.DeleteThing | TestModuleAAccessOption.EditThing)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleAccess(TestModuleAAccessOption.DeleteThing)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleAccess(TestModuleAAccessOption.DeleteThing)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModuleAAccessOption
        {
            EditThing = 1,
            DeleteThing = 2
        }

        // todo: how to send data from frontend here? only allow post?
        // => [controller]/module/<name>/<action>
        // todo: how to receive it? find matching module & method w/ parameter & deserialize payload into single parameter?
        //       serialize result from method if any. Allow async methods.
        // => /module/test/editThing => public x EditThing

        /*
         ToDo:
         - override id and name when calling UseModule(..)
         - warn in frontend if any invalid modules are registered, any invalid methods, verify that enum is flags etc
         */
    }
}
