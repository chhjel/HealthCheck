﻿using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheck.Dev.Common.Tests
{
    [RuntimeTestClass(
        Name = "Class proxy tests",
        DefaultRolesWithAccess = RuntimeTestAccessRole.WebAdmins,
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
            
            return new ProxyRuntimeTestConfig(typeof(SomeService))
                .AddParameterTypeConfig<SomeParameterType>(
                    choicesFactory: () => getUserChoices().Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Name)),
                    getInstanceByIdFactory: (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                );
        }

        internal class SomeService
        {
#pragma warning disable S3400 // Methods should not return constants
            public string WithReturnValue() => "Success!";
#pragma warning restore S3400 // Methods should not return constants

#pragma warning disable IDE0060 // Remove unused parameter
            public void WithParameter(string data) { /**/ }
#pragma warning restore IDE0060 // Remove unused parameter

            public string WithParameterAndReturnValue(string data) => $"Input was {data}";

            public async Task<string> WithParameterAndReturnValueAsync(string data)
            {
                await Task.Delay(500);
                return $"Input was {data}";
            }

            public SomeParameterType WithReferenceParameter(SomeParameterType someType) => someType;
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
                    () => getUserChoices().Select(x => new RuntimeTestReferenceParameterChoice(x.Id.ToString(), x.Name)),
                    (id) => getUserChoices().FirstOrDefault(x => x.Id.ToString() == id)
                )
            };
        }
    }
}
