using HealthCheck.Core.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Core.TestManagers
{
    /// <summary>
    /// Executes tests.
    /// </summary>
    public class TestRunner
    {
        /// <summary>
        /// Include exception stacktraces in test results.
        /// <para>False by default.</para>
        /// </summary>
        public bool IncludeExceptionStackTraces { get; set; }

        /// <summary>
        /// Execute all the tests in the given test classes.
        /// </summary>
        public async Task<List<TestResult>> ExecuteTests(List<TestClassDefinition> testClasses)
        {
            var results = new ConcurrentBag<TestResult>();
            foreach (var testClass in testClasses)
            {
                var classInstance = Activator.CreateInstance(testClass.ClassType);

                var defaultAllowsParallel = testClass.DefaultAllowParallelExecution == true;
                var testsThatCanRunInParallel = testClass.Tests
                    .Where(x =>
                        (defaultAllowsParallel
                        && (x.AllowParallelExecution == null || x.AllowParallelExecution == true))
                        || (!defaultAllowsParallel
                        && (x.AllowParallelExecution != null && x.AllowParallelExecution == true))
                    );
                var testsThatCannotRunInParallel = testClass.Tests.Except(testsThatCanRunInParallel);

                // Run tests that are allowed to run in parallel first
                if (testsThatCanRunInParallel.Any())
                {
                    var tasks = new List<Task<TestResult>>();
                    foreach (var test in testsThatCanRunInParallel)
                    {
                        var task = ExecuteTest(test, null, classInstance);
                        tasks.Add(task);
                    }
                    await Task.WhenAll(tasks);
                }

                // Run other tests after
                foreach (var test in testsThatCannotRunInParallel)
                {
                    var result = await ExecuteTest(test, null, classInstance);
                    results.Add(result);
                }
            }
            return results.ToList();
        }

        /// <summary>
        /// Execute the given test with optional parameters and instance.
        /// </summary>
        public async Task<TestResult> ExecuteTest(TestDefinition test,
            object[] parameters = null,
            object testClassInstance = null)
        {
            try
            {
                var instance = testClassInstance ?? Activator.CreateInstance(test.ParentClass.ClassType);
                var result = await test.ExecuteTest(instance, parameters);

                // Post-process result
                result.Test = test;
                if (!IncludeExceptionStackTraces)
                {
                    result.StackTrace = null;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new TestResult()
                {
                    Test = test,
                    Status = Enums.TestResultStatus.Error,
                    Message = $"Failed to execute test with the exception: {ex.Message}",
                    StackTrace = IncludeExceptionStackTraces ? ex.ToString() : null
                };
            }
        }
    }
}
