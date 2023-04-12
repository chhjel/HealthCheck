using QoDL.Toolkit.Core.Abstractions.Modules;
using System;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Module.DevModule
{
    /// <summary>
    /// A broken module for testing.
    /// </summary>
    public class TestModuleB : ToolkitModuleBase<TestModuleB.TestModuleBAccessOption>
    {
        public override object GetFrontendOptionsObject(ToolkitModuleContext context)
            => new { PropX = "asdasd asd", PropY = 135135, Now = DateTime.Now, AccessInput = context.CurrentRequestModuleAccessOptions.ToString() };
        public override IToolkitModuleConfig GetModuleConfig(ToolkitModuleContext context) => null;

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestSimple()
        {
            // Method intentionally left empty.
        }

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestSimple(int id) { Console.WriteLine(id); }

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberOne)]
        public void TestNoReturn(int id) { Console.WriteLine(id); }

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberOne)]
        public object Test(int id) => new { Success = true, Message = $"Your id is '{id}'." };

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberOne)]
        public string TestSimpleReturn(int id) => $"Your id is '{id}'.";

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task TestSimpleAsync() => await Task.Delay(10);

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberTwo | TestModuleBAccessOption.NumberOne)]
        public async Task TestNoReturnAsync(int id) => await Task.Delay((id * 0) + 1);

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task<object> TestAsync(int id) => await Task.FromResult(new { Success = true, Message = $"Your id is '{id}'." });

        [ToolkitModuleMethod(TestModuleBAccessOption.NumberTwo)]
        public async Task<string> TestSimpleReturnAsync(int id) => await Task.FromResult($"Your id is '{id}'.");

        public enum TestModuleBAccessOption
        {
            NumberOne = 1,
            NumberTwo = 2
        }
    }
}
