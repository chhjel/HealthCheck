using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCheck.Core.TestManagers
{
    public class TestRunnerTests
    {
        [Fact]
        public async Task ExecuteTests_WithAFewTests_ReturnsResults()
        {
            var discoverer = new TestDiscoverer()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var testClasses = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetA")
                .ToList();

            var runner = new TestRunner();
            var results = await runner.ExecuteTests(testClasses);
            Assert.NotEmpty(results);
        }

        [Fact]
        public async Task ExecuteTests_WithAsyncAndTaskTest_DoesNotFail()
        {
            var discoverer = new TestDiscoverer()
            {
                AssemblyContainingTests = GetType().Assembly
            };
            var testClasses = discoverer.DiscoverTestDefinitions()
                .Where(x => x.Id == "TestRunnerTestsSetB")
                .ToList();

            var runner = new TestRunner();
            var results = await runner.ExecuteTests(testClasses);
            Assert.NotEmpty(results);
            Assert.Contains(results, x => x.Tag as string == "AsyncTestMethod");
            Assert.Contains(results, x => x.Tag as string == "TaskTestMethod");
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetA", Description = "Some test set", Name = "Dev test set")]
        public class TestClassA
        {
            [RuntimeTest]
            public TestResult TestMethodWithoutParameters()
            {
                return new TestResult();
            }

            [RuntimeTest(ParameterDescriptions = new[] { "a desc", "b", "c" })]
            public TestResult TestMethodWithParameters(string stringArg = "wut", bool boolArg = true, int intArg = 123)
            {
                return new TestResult()
                {
                    Tag = new object[] { stringArg, boolArg, intArg }
                };
            }
        }

        [RuntimeTestClass(Id = "TestRunnerTestsSetB", Description = "Some test set", Name = "Dev test set")]
        public class TestClassB
        {
            [RuntimeTest]
            public async Task<TestResult> AsyncTestMethod()
            {
                await Task.Delay(100);
                return new TestResult() { Tag = "AsyncTestMethod" };
                }

            [RuntimeTest]
            public Task<TestResult> TaskTestMethod()
            {
                return Task.FromResult(new TestResult() { Tag = "TaskTestMethod" });
            }
        }
    }
}
