using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Class proxy tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.SystemAdmins,
        GroupName = RuntimeTestConstants.Group.AlmostTopGroup,
        UIOrder = 30
    )]
    public class ClassProxyTests
    {
        [ProxyRuntimeTests]
        public static ProxyRuntimeTestConfig ProxyTest()
        {
            var getUserChoices = new Func<IEnumerable<SomeParameterType>>(() => new[] {
                new SomeParameterType(1, "Jimmy Smithy"),
                new SomeParameterType(2, "Bob Rob"),
                new SomeParameterType(3, "Elly Nelly")
            });

            var getOtherChoices = new Func<Type, IEnumerable<SomeOtherParameterType>>((type) =>
            {
                if (type == typeof(SomeOtherParameterType))
                {
                    return new[] {
                        new SomeOtherParameterType(4, "Jimmy Smithy Other"),
                        new SomeOtherParameterType(5, "Bob Rob Other"),
                        new SomeOtherParameterType(6, "Elly Nelly Other")
                    };
                }
                else
                {
                    return new[] {
                        new SomeOtherSubParameterType(7, "Jimmy Smithy Other Sub"),
                        new SomeOtherSubParameterType(8, "Bob Rob Other Sub"),
                        new SomeOtherSubParameterType(9, "Elly Nelly Other Sub")
                    };
                }
            });

            return new ProxyRuntimeTestConfig(typeof(SomeService))
                .SetCustomContext(
                    contextFactory: () => new { MemoryLogger = new HCMemoryLogger() },
                    instanceFactory: (context) => new SomeService(context.MemoryLogger),
                    resultAction: (result, context) =>
                    {
                        result
                            .AddCodeData(context.MemoryLogger.Contents)
                            .ForProxyResult<SomeParameterType>((value) => result.AddTextData("Is of type SomeParameterType!"))
                            .AddTextData(result.ProxyTestResultObject?.GetType()?.Name ?? "null", "Result type");
                    }
                )
                .AddParameterTypeConfig<SomeParameterType>(
                    choicesFactory: (filter) => getUserChoices()
                        .Where(x => x.Name.Contains(filter))
                        .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Name)),
                    getInstanceByIdFactory: (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                )
                .AddParameterTypeConfig<SomeOtherParameterType>(
                    choicesFactoryByType: (type, filter)
                        => getOtherChoices(type)
                            .Where(x => x.Name.Contains(filter))
                            .Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Name)),
                    getInstanceByIdFactoryByType: (type, id) => getOtherChoices(type).FirstOrDefault(x => x.Id.ToString() == id)
                );
        }

        internal class SomeService
        {
            private readonly HCMemoryLogger _logger;

            public SomeService() {}
            public SomeService(HCMemoryLogger logger) => _logger = logger;

            public async Task WithTaskReturnValue() => await Task.Delay(100);

#pragma warning disable S1186 // Methods should not be empty
            public void WithVoidReturnValue() {}
#pragma warning restore S1186 // Methods should not be empty

            public SomeParameterType WithComplexReturnValue() => new SomeParameterType(42, "Test");

#pragma warning disable S3400 // Methods should not return constants
            public string WithReturnValue()
            {
                _logger?.Error("A logged error test");
                return $"Success!";
            }
#pragma warning restore S3400 // Methods should not return constants

#pragma warning disable IDE0060 // Remove unused parameter
            public void WithParameter(Guid id, string data) { /**/ }
#pragma warning restore IDE0060 // Remove unused parameter

            public string WithParameterAndReturnValue1(Guid id) => $"Input was {id}";

            public string WithParameterAndReturnValue2(Guid? id) => $"Input was {id?.ToString() ?? "null"}";

            public async Task<DateTimeOffset?> NullableTaskResultTest()
            {
                await Task.Delay(100);
                return DateTimeOffset.UtcNow;
            }

            public async Task<string> WithParameterAndReturnValueAsync(string data)
            {
                await Task.Delay(500);
                return $"Input was {data}";
            }

            public SomeParameterType WithReferenceParameter(SomeParameterType someType) => someType;

            public SomeOtherParameterType WithInheritedReferenceTypes1(SomeParameterType someType, SomeOtherParameterType otherType, SomeOtherSubParameterType otherSubType)
                => otherType;

            public SomeOtherParameterType WithInheritedReferenceTypes2(SomeParameterType someType, SomeOtherParameterType otherType, SomeOtherSubParameterType otherSubType)
                => otherSubType;
        }

        public class SomeParameterType
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public SomeParameterType(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        public class SomeOtherParameterType
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public SomeOtherParameterType(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }

        public class SomeOtherSubParameterType : SomeOtherParameterType
        {
            public string SubParam { get; set; } = "123";

            public SomeOtherSubParameterType(int id, string name) : base(id, name)
            {
            }
        }

        [RuntimeTest(ReferenceParameterFactoryProviderMethodName = nameof(GetReferenceFactories))]
        public TestResult ReferenceParameterTest(SomeParameterType data)
        {
            return TestResult.CreateSuccess($"Selected data was: {data?.Name}");
        }

        [RuntimeTest]
        public TestResult MixedWithANonProxy(int number)
        {
            return TestResult.CreateSuccess($"Number {number} is a success!");
        }

        public static List<RuntimeTestReferenceParameterFactory> GetReferenceFactories()
        {
            var getUserChoices = new Func<IEnumerable<SomeParameterType>>(() =>
                Enumerable.Range(1, 1000).Select(x => new SomeParameterType(x, $"User #{x}"))
            );

            return new List<RuntimeTestReferenceParameterFactory>()
            {
                new RuntimeTestReferenceParameterFactory(
                    typeof(SomeParameterType),
                    (filter) => getUserChoices().Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Name)),
                    (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                )
            };
        }
    }
}
