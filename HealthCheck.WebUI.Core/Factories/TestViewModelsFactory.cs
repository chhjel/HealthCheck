using HealthCheck.Core.Entities;
using HealthCheck.Core.Util;
using HealthCheck.Web.Core.ViewModels;
using RuntimeCodeTest.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Web.Core.Factories
{
    /// <summary>
    /// View model object factory for test objects.
    /// </summary>
    public class TestViewModelsFactory
    {
        /// <summary>
        /// Create a <see cref="TestSetViewModel"/> from the given list of <see cref="TestClassDefinition"/>.
        /// </summary>
        public List<TestSetViewModel> CreateViewModel(IEnumerable<TestClassDefinition> testClassDefinitions)
            => testClassDefinitions.Select(x => CreateViewModel(x)).ToList();

        /// <summary>
        /// Create a <see cref="TestSetViewModel"/> from the given <see cref="TestClassDefinition"/>.
        /// </summary>
        public TestSetViewModel CreateViewModel(TestClassDefinition testClassDefinition)
        {
            var vm = new TestSetViewModel()
            {
                Id = testClassDefinition.Id,
                Name = testClassDefinition.Name,
                Description = testClassDefinition.Description,
                Tests = new List<TestViewModel>()
            };

            foreach (var test in testClassDefinition.Tests)
            {
                vm.Tests.Add(CreateViewModel(test));
            }

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestViewModel"/> from the given <see cref="TestDefinition"/>.
        /// </summary>
        public TestViewModel CreateViewModel(TestDefinition testDefinition)
        {
            var vm = new TestViewModel()
            {
                Id = testDefinition.Id,
                Name = testDefinition.Name,
                Description = testDefinition.Description,
                Parameters = new List<TestParameterViewModel>()
            };

            foreach (var parameter in testDefinition.Parameters)
            {
                vm.Parameters.Add(CreateViewModel(parameter));
            }

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestParameterViewModel"/> from the given <see cref="TestParameter"/>.
        /// </summary>
        public TestParameterViewModel CreateViewModel(TestParameter testParameter)
        {
            var vm = new TestParameterViewModel()
            {
                Name = testParameter.Name,
                Description = testParameter.Description,
                DefaultValue = testParameter.DefaultValue?.ToString(),
                Type = testParameter.ParameterType.Name
            };

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestResultViewModel"/> from the given <see cref="TestResult"/>.
        /// </summary>
        public TestResultViewModel CreateViewModel(TestResult testResult)
        {
            var dumps = testResult.Data.Select(x => CreateViewModel(x)).ToList();

            var vm = new TestResultViewModel()
            {
                TestId = testResult.Test.Id,
                StatusCode = (int)testResult.Status,
                Status = testResult.Status,
                Message = testResult.Message,
                Data = dumps
            };

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestResultDataDumpViewModel"/> from the given <see cref="TestResultDataDump"/>.
        /// </summary>
        public TestResultDataDumpViewModel CreateViewModel(TestResultDataDump dataDump)
        {
            var vm = new TestResultDataDumpViewModel()
            {
                Title = dataDump.Title,
                Data = dataDump.Data,
                Type = dataDump.Type
            };

            return vm;
        }
    }

}
