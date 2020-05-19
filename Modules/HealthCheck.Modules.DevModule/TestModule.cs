using HealthCheck.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace HealthCheck.Modules.DevModule
{
    public class TestModule : HealthCheckModuleBase<TestModule.TestModuleAccessOption>
    {
        public override object GetFrontendOptionsObject() => new { PropA = "something", PropB = 123 };
        public override IHealthCheckModuleConfig GetModuleConfig() => new TestModuleConfig();

        [HealthCheckModuleAccess(TestModuleAccessOption.EditThing)]
        public void TestSimple() { }

        [HealthCheckModuleAccess(TestModuleAccessOption.EditThing)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [HealthCheckModuleAccess(TestModuleAccessOption.EditThing)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [HealthCheckModuleAccess(TestModuleAccessOption.EditThing)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [HealthCheckModuleAccess(TestModuleAccessOption.DeleteThing)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [HealthCheckModuleAccess(TestModuleAccessOption.DeleteThing | TestModuleAccessOption.EditThing)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [HealthCheckModuleAccess(TestModuleAccessOption.DeleteThing)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [HealthCheckModuleAccess(TestModuleAccessOption.DeleteThing)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModuleAccessOption
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
         - iterate over Modules and discover
           * GetFrontendOptionsObject
           * GetModuleConfig
           * Methods with [HealthCheckModuleAccessAttribute]
             > allow async
             > with/without return value
             > a single parameter that is serializable to json, complex or not
             > for each method that user has access to generate the url for it automatically and include in frontend data
              { "MethodName": "http...", "AnotherMethod": "http..." }
         - warn in frontend if any invalid modules are registered, any invalid methods, verify that enum is flags etc
         */
    }
}
