using HealthCheck.Core.Entities;
using HealthCheck.Core.Extensions;
using HealthCheck.WebUI.Models;
using HealthCheck.WebUI.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.WebUI.Factories
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
                GroupName = testClassDefinition.GroupName,
                UIOrder = testClassDefinition.UIOrder,
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
                Type = testParameter.ParameterType.GetFriendlyTypeName()
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
                TestName = testResult.Test.Name,
                StatusCode = (int)testResult.Status,
                Status = testResult.Status,
                Message = testResult.Message,
                StackTrace = testResult.StackTrace,
                DurationInMilliseconds = testResult.DurationInMilliseconds,
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

        /// <summary>
        /// Create a list of <see cref="GroupOptionsViewModel"/> from the given <see cref="TestSetGroupsOptions"/>.
        /// </summary>
        public List<GroupOptionsViewModel> CreateViewModel(TestSetGroupsOptions groupOptions)
        {
            return groupOptions.GetOptions().Select(x => new GroupOptionsViewModel()
            {
                GroupName = x.GroupName,
                UIOrder = x.UIOrder
            }).ToList();
        }

        /// <summary>
        /// Create a <see cref="GroupOptionsViewModel"/> from the given <see cref="TestSetGroupsOptions"/>.
        /// </summary>
        internal AuditEventViewModel CreateViewModel(AuditEvent e)
        {
            return new AuditEventViewModel()
            {
                Timestamp = e.Timestamp,
                Area = e.Area,
                AreaCode = (int)e.Area,
                Action = e.Action,
                Subject = e.Subject,
                Details = e.Details,
                UserId = e.UserId,
                UserName = e.UserName,
                UserAccessRoles = e.UserAccessRoles,
            };
        }
    }

}
