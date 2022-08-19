using HealthCheck.Core.Attributes;
using HealthCheck.Core.Config;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Modules.Tests.Utils;
using HealthCheck.Core.Util;
using HealthCheck.Core.Util.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
        public List<TestSetViewModel> CreateViewModel(IEnumerable<TestClassDefinition> testClassDefinitions, HCTestsModuleOptions options, List<string> userCategoryAccess)
            => testClassDefinitions.Select(x => CreateViewModel(x, options, userCategoryAccess)).ToList();

        /// <summary>
        /// Create a <see cref="TestSetViewModel"/> from the given <see cref="TestClassDefinition"/>.
        /// </summary>
        public TestSetViewModel CreateViewModel(TestClassDefinition testClassDefinition, HCTestsModuleOptions options, List<string> userCategoryAccess)
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
                vm.Tests.AddRange(CreateViewModels(test, options, userCategoryAccess));
            }

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestViewModel"/> from the given <see cref="TestDefinition"/>.
        /// </summary>
        public IEnumerable<TestViewModel> CreateViewModels(TestDefinition testDefinition, HCTestsModuleOptions options, List<string> userCategoryAccess)
        {
            List<TestViewModel> viewModels = new();

            var model = CreateViewModel(testDefinition, options, userCategoryAccess);
            viewModels.Add(model);

            return viewModels;
        }

        private TestViewModel CreateViewModel(TestDefinition testDefinition, HCTestsModuleOptions options, List<string> userCategoryAccess)
        {
            var categories = testDefinition.Categories ?? new List<string>();
            if (userCategoryAccess?.Any() == true)
            {
                categories = categories.Where(x => userCategoryAccess.Contains(x)).ToList();
            }

            var model = new TestViewModel()
            {
                Id = testDefinition.Id,
                Name = testDefinition.Name,
                Description = testDefinition.Description,
                RunButtonText = testDefinition.RunButtonText,
                RunningButtonText = testDefinition.RunningButtonText,
                IsCancellable = testDefinition.IsCancellable,
                Parameters = new List<TestParameterViewModel>(),
                Categories = categories
            };

            foreach (var parameter in testDefinition.Parameters)
            {
                model.Parameters.Add(CreateViewModel(parameter, options));
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
        public TestParameterViewModel CreateViewModel(TestParameter testParameter, HCTestsModuleOptions options)
        {
            var stringConverter = new HCStringConverter();
            var paramType = testParameter.ParameterType;
            string type = HCCustomPropertyAttribute.CreateParameterTypeName(paramType);

            var hidden = testParameter.IsOut
                || testParameter.ParameterType.IsGenericParameter
                || options?.HideInputForTypes?.Any(x => x.IsAssignableFrom(testParameter.ParameterType)) == true;

            var vm = new TestParameterViewModel()
            {
                Index = testParameter.Index,
                Name = testParameter.Name,
                Description = testParameter.Description,
                DefaultValue = stringConverter.ConvertToString(testParameter.DefaultValue),
                PossibleValues = testParameter?.PossibleValues?.Select(x => stringConverter.ConvertToString(x))?.ToList(),
                Type = type,
                UIHints = testParameter?.UIHints ?? new(),
                NullName = testParameter.NullName,
                TextPattern = HCBackendInputConfig.EnsureJsRegexIsWrappedIfNotEmpty(testParameter?.TextPattern),
                IsCustomReferenceType = testParameter.IsCustomReferenceType,
                Hidden = hidden,
                ReferenceValueFactoryConfig = CreateReferenceValueFactoryConfig(testParameter.ReferenceFactory)
            };

            return vm;
        }

        private ReferenceValueFactoryConfigViewModel CreateReferenceValueFactoryConfig(RuntimeTestReferenceParameterFactory referenceFactory)
        {
            if (string.IsNullOrWhiteSpace(referenceFactory?.Title) 
                && string.IsNullOrWhiteSpace(referenceFactory?.Description)
                && string.IsNullOrWhiteSpace(referenceFactory?.SearchButtonText))
            {
                return null;
            }

            return new ReferenceValueFactoryConfigViewModel
            {
                Title = referenceFactory.Title,
                Description = referenceFactory.Description,
                SearchButtonText = referenceFactory.SearchButtonText                
            };
        }


        /// <summary>
        /// Create template values for any unsupported types.
        /// </summary>
        public List<TestParameterTemplateViewModel> CreateParameterTemplatesViewModel(List<TestClassDefinition> testDefinitions, HCTestsModuleOptions options)
        {
            var parameters = testDefinitions.SelectMany(x => x.Tests.SelectMany(t => t.Parameters));
            var relevantParameters = parameters
                .Where(x => !x.IsOut
                    && !x.IsCustomReferenceType 
                    && !TestsModuleUtils.IsBuiltInSupportedType(x.ParameterType))
                .GroupBy(x => x.ParameterType.Name)
                .Select(x => x.First())
                .ToArray();

            return relevantParameters.Select(x =>
            {
                string template = null;
                try
                {
                    var useDefaultLogic = true;
                    if (options.JsonInputTemplateFactory != null)
                    {
                        var factoryResult = options.JsonInputTemplateFactory(x.ParameterType);
                        if (factoryResult?.NoTemplate == true)
                        {
                            template = "";
                            useDefaultLogic = false;
                        }
                        else if (factoryResult?.Instance != null)
                        {
                            template = TestRunnerService.Serializer?.Serialize(factoryResult.Instance);
                            useDefaultLogic = false;
                        }
                    }
                    
                    if (useDefaultLogic && HCGlobalConfig.AllowActivatingType(x.ParameterType) && HCGlobalConfig.AllowSerializingType(x.ParameterType))
                    {
                        var ctors = x.ParameterType.GetConstructors();
                        var hasParameterlessConstructor = ctors?.Any() != true || ctors?.Any(c => c.GetParameters().Length == 0) == true;
                        if (hasParameterlessConstructor)
                        {
                            var instance = Activator.CreateInstance(x.ParameterType);
                            template = TestRunnerService.Serializer?.Serialize(instance);
                        }
                    }
                }
                catch (Exception)
                {
                    try
                    {
                        var instance = FormatterServices.GetUninitializedObject(x.ParameterType);
                        template = TestRunnerService.Serializer?.Serialize(instance);
                    }
                    catch (Exception) { /* Ignored */ }
                }

                return new TestParameterTemplateViewModel
                {
                    Type = HCCustomPropertyAttribute.CreateParameterTypeName(x.ParameterType),
                    Template = template ?? "{\n}"
                };
            }).ToList();
        }

        /// <summary>
        /// Create a <see cref="TestResultViewModel"/> from the given <see cref="TestResult"/>.
        /// </summary>
        public TestResultViewModel CreateViewModel(TestResult testResult)
        {
            var dumps = testResult.Data.Select(x => CreateViewModel(x)).ToList();
            var parameterFeedback = new Dictionary<int, string>();
            if (testResult.FeedbackPerParameter != null)
            {
                foreach (var parameter in testResult.Test.Parameters)
                {
                    var feedback = testResult.FeedbackPerParameter(parameter.Id);
                    if (!string.IsNullOrWhiteSpace(feedback))
                    {
                        var index = testResult.Test.Parameters.FirstOrDefault(x => x.Id == parameter.Id)?.Index ?? -1;
                        parameterFeedback[index] = feedback;
                    }
                }
            }
            foreach (var kvp in testResult.ParameterFeedback)
            {
                var index = testResult.Test.Parameters.FirstOrDefault(x => x.Id == kvp.Key)?.Index ?? -1;
                parameterFeedback[index] = kvp.Value;
            }

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
                ParameterFeedback = parameterFeedback,
                ExpandDataByDefault = testResult.ExpandDataByDefault || !testResult.AllowExpandData,
                DurationInMilliseconds = testResult.DurationInMilliseconds,
                Data = dumps,
                InputWasAllowedAuditLogged = !testResult.Test.HideInputFromAuditLog,
                ResultMessageWasAllowedAuditLogged = !testResult.Test.HideResultMessageFromAuditLog
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
                Type = dataDump.Type,
                DisplayClean = dataDump.DisplayClean,
                DownloadFileName = dataDump.DownloadFileName,
                Flags = dataDump.Flags ?? new List<string>()
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
