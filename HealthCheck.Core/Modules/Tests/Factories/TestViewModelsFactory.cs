using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheck.Core.Modules.Tests.Factories
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
                AllowRunAll = testClassDefinition.AllowRunAll,
                Tests = new List<TestViewModel>(),
            };

            foreach (var test in testClassDefinition.Tests.Where(x => x.LoadErrors?.Any() != true))
            {
                vm.Tests.AddRange(CreateViewModels(test));
            }

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestViewModel"/> from the given <see cref="TestDefinition"/>.
        /// </summary>
        public IEnumerable<TestViewModel> CreateViewModels(TestDefinition testDefinition)
        {
            List<TestViewModel> viewModels = new List<TestViewModel>();

            var model = CreateViewModel(testDefinition);
            viewModels.Add(model);

            return viewModels;
        }

        private TestViewModel CreateViewModel(TestDefinition testDefinition)
        {
            var model = new TestViewModel()
            {
                Id = testDefinition.Id,
                Name = testDefinition.Name,
                Description = testDefinition.Description,
                RunButtonText = testDefinition.RunButtonText,
                RunningButtonText = testDefinition.RunningButtonText,
                IsCancellable = testDefinition.IsCancellable,
                Parameters = new List<TestParameterViewModel>()
            };

            foreach (var parameter in testDefinition.Parameters)
            {
                model.Parameters.Add(CreateViewModel(parameter));
            }

            return model;
        }

        /// <summary>
        /// Create a <see cref="InvalidTestViewModel"/> from the given <see cref="TestDefinitionValidationResult"/>.
        /// </summary>
        public InvalidTestViewModel CreateViewModel(TestDefinitionValidationResult testDefinition)
        {
            var vm = new InvalidTestViewModel()
            {
                Id = testDefinition.Test.Id,
                Name = testDefinition.Test.Name,
                Reason = testDefinition.Error
            };

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestParameterViewModel"/> from the given <see cref="TestParameter"/>.
        /// </summary>
        public TestParameterViewModel CreateViewModel(TestParameter testParameter)
        {
            var stringConverter = new StringConverter();
            var paramType = testParameter.ParameterType;
            string type = paramType.GetFriendlyTypeName();
            if (paramType.IsEnum)
            {
                type = EnumUtils.IsTypeEnumFlag(paramType) ? "FlaggedEnum" : "Enum";
            }
            else if (paramType.IsGenericType
                && paramType.GetGenericTypeDefinition() == typeof(List<>)
                && paramType.GetGenericArguments()[0].IsEnum)
            {
                var innerType = EnumUtils.IsTypeEnumFlag(paramType.GetGenericArguments()[0]) ? "FlaggedEnum" : "Enum";
                type = $"List<{innerType}>";
            }

            var vm = new TestParameterViewModel()
            {
                Index = testParameter.Index,
                Name = testParameter.Name,
                Description = testParameter.Description,
                DefaultValue = stringConverter.ConvertToString(testParameter.DefaultValue),
                PossibleValues = testParameter?.PossibleValues?.Select(x => stringConverter.ConvertToString(x))?.ToList(),
                Type = type,
                NotNull = testParameter.NotNull,
                ReadOnlyList = testParameter.ReadOnlyList,
                ShowTextArea = testParameter.ShowTextArea,
                FullWidth = testParameter.FullWidth,
                IsCustomReferenceType = testParameter.IsCustomReferenceType
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
                AllowExpandData = testResult.AllowExpandData,
                DisplayClean = testResult.DisplayClean,
                ExpandDataByDefault = testResult.ExpandDataByDefault || !testResult.AllowExpandData,
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
    }

}
