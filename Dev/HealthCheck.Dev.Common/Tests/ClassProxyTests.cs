using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Utility.Reflection.Logging;
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
        UIOrder = 80
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
                .SetMethodFilter(x => !x.Name.StartsWith("FilteredOut"))
                .SetCustomContext(
                    contextFactory: () => new { MemoryLogger = HCLogTypeBuilder.CreateMemoryLoggerFor<ISomeLogger>() },
                    instanceFactory: (context) => new SomeService(context.MemoryLogger),
                    resultAction: (result, context) =>
                    {
                        result
                            .AddCodeData(context.MemoryLogger.ToString())
                            .ForProxyResult<SomeParameterType>((value) => result.AddTextData("Is of type SomeParameterType!"));
                            //.AddTextData(result.ProxyTestResultObject?.GetType()?.Name ?? "null", "Result type")
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

        public interface ISomeLogger
        {
            void Error(string msg, Exception ex = null);
        }
        internal class SomeService
        {
            private readonly ISomeLogger _logger;

            public SomeService() {}
            public SomeService(ISomeLogger logger) => _logger = logger;

            public async Task WithTaskReturnValue() => await Task.Delay(100);

            public void WithVoidReturnValue() {}

            public SomeParameterType WithComplexReturnValue() => new SomeParameterType(42, "Test");

            public string WithReturnValue()
            {
                _logger?.Error("A logged error test");
                return $"Success!";
            }

            public string WithExceptionLogged()
            {
                try
                {
                    int.Parse("asd");
                }
                catch(Exception ex)
                {
                    _logger?.Error("Testing logged exception here.", ex);
                }
                return $"Success!";
            }

            public void WithParameter(Guid id, string data) { /**/ }

            public string With1GenericArgument<T1>(T1 a) => $"Input was {a}";
            public string With2GenericArguments<T1, T2>(T1 a, T2 b) => $"Input was {a}, {b}";
            public string With3GenericArguments<T1, T2, T3>(T1 a, T2 b, T3 c) => $"Input was {a}, {b}, {c}";

            public string WithParameterAndReturnValue1(Guid id) => $"Input was {id}";

            public string WithParameterAndReturnValue2(Guid? id) => $"Input was {id?.ToString() ?? "null"}";

            public string FilteredOutMethod(Guid id) => $"Input was {id}";

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
