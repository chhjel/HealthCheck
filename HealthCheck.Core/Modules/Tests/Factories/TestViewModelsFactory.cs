using HealthCheck.Core.Config;
using HealthCheck.Core.Extensions;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Modules.Tests.Utils;
using HealthCheck.Core.Util;
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
        public List<TestSetViewModel> CreateViewModel(IEnumerable<TestClassDefinition> testClassDefinitions, HCTestsModuleOptions options)
            => testClassDefinitions.Select(x => CreateViewModel(x, options)).ToList();

        /// <summary>
        /// Create a <see cref="TestSetViewModel"/> from the given <see cref="TestClassDefinition"/>.
        /// </summary>
        public TestSetViewModel CreateViewModel(TestClassDefinition testClassDefinition, HCTestsModuleOptions options)
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
                vm.Tests.AddRange(CreateViewModels(test, options));
            }

            return vm;
        }

        /// <summary>
        /// Create a <see cref="TestViewModel"/> from the given <see cref="TestDefinition"/>.
        /// </summary>
        public IEnumerable<TestViewModel> CreateViewModels(TestDefinition testDefinition, HCTestsModuleOptions options)
        {
            List<TestViewModel> viewModels = new List<TestViewModel>();

            var model = CreateViewModel(testDefinition, options);
            viewModels.Add(model);

            return viewModels;
        }

        private TestViewModel CreateViewModel(TestDefinition testDefinition, HCTestsModuleOptions options)
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
            var stringConverter = new StringConverter();
            var paramType = testParameter.ParameterType;
            string type = CreateParameterTypeName(paramType);

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
                NotNull = testParameter.NotNull,
                ReadOnlyList = testParameter.ReadOnlyList,
                ShowTextArea = testParameter.ShowTextArea,
                ShowCodeArea = testParameter.ShowCodeArea,
                FullWidth = testParameter.FullWidth,
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

        private static readonly Dictionary<string, string> _inputTypeAliases = new Dictionary<string, string>
        {
            { "IFormFile", "HttpPostedFileBase" },
            { "Byte[]", "HttpPostedFileBase" }
        };
        private string CreateParameterTypeName(Type type)
        {
            var typeName = type.GetFriendlyTypeName(_inputTypeAliases);
            if (type.IsEnum)
            {
                typeName = EnumUtils.IsTypeEnumFlag(type) ? "FlaggedEnum" : "Enum";
            }
            else if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(List<>)
                && type.GetGenericArguments()[0].IsEnum)
            {
                var innerType = EnumUtils.IsTypeEnumFlag(type.GetGenericArguments()[0]) ? "FlaggedEnum" : "Enum";
                typeName = $"List<{innerType}>";
            }
            return typeName;
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
                    Type = CreateParameterTypeName(x.ParameterType),
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
                Type = dataDump.Type,
                DisplayClean = dataDump.DisplayClean,
                DownloadFileName = dataDump.DownloadFileName
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
