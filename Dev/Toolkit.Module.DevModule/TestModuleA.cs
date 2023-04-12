using QoDL.Toolkit.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DevModule
{
    public class TestModuleA : ToolkitModuleBase<TestModuleA.TestModuleAAccessOption>
    {
        public override object GetFrontendOptionsObject(ToolkitModuleContext context)
            => new { PropA = "something", PropB = 123, AccessInput = context.CurrentRequestModuleAccessOptions.ToString() };
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context)
            => new TestModuleAConfig();

        [ToolkitModuleMethod(TestModuleAAccessOption.EditThing)]
        public void TestSimple() { /* Does nothing, just for testing. */ }

        [ToolkitModuleMethod(TestModuleAAccessOption.EditThing)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [ToolkitModuleMethod(TestModuleAAccessOption.EditThing)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [ToolkitModuleMethod(TestModuleAAccessOption.EditThing)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [ToolkitModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [ToolkitModuleMethod(TestModuleAAccessOption.DeleteThing | TestModuleAAccessOption.EditThing)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [ToolkitModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [ToolkitModuleMethod(TestModuleAAccessOption.DeleteThing)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        [Flags]
        public enum TestModuleAAccessOption
        {
            EditThing = 1,
            DeleteThing = 2
        }
    }
}
