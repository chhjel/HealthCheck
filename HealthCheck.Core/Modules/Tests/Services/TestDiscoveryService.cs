using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HealthCheck.Core.Modules.Tests.Services
{
    /// <summary>
    /// Discovers and validates test methods and test sets.
    /// </summary>
    public class TestDiscoveryService
    {
        /// <summary>
        /// The assemblies that contains the test methods.
        /// </summary>
        public IEnumerable<Assembly> AssembliesContainingTests { get; set; }

        /// <summary>
        /// Used to support custom reference types.
        /// </summary>
        public List<RuntimeTestReferenceParameterFactory> ReferenceParameterFactories { get; set; } = new List<RuntimeTestReferenceParameterFactory>();

        /// <summary>
        /// Allow proxy tests to be discovered.
        /// </summary>
        public bool IncludeProxyTests { get; set; } = true;

        /// <summary>
        /// Create a new <see cref="TestDiscoveryService"/>.
        /// </summary>
        public TestDiscoveryService() {}

        /// <summary>
        /// Validate tests, throws a <see cref="InvalidTestDefinitionException"/> if any tests are invalid.
        /// </summary>
        public void ValidateTests()
        {
            var tests = GetInvalidTests();
            if (!tests.Any()) return;

            var message = string.Join(Environment.NewLine, tests.Select(x => x.Error));
            throw new InvalidTestDefinitionException(message);
        }

        /// <summary>
        /// Get a list of tests that failed validation.
        /// </summary>
        public List<TestDefinitionValidationResult> GetInvalidTests()
        {
            var testClasses = DiscoverTestDefinitions(includeInvalidTests: true);
            return testClasses
                .SelectMany(x => x.Tests)
                .Select(x => x.Validate())
                .Where(x => !x.IsValid)
                .ToList();
        }

        /// <summary>
        /// Discover tests, only returning ones that have any of the given roles.
        /// </summary>
        public List<TestClassDefinition> DiscoverTestDefinitions<TAccessRolesEnum>(TAccessRolesEnum userRoles,
            bool includeInvalidTests = false,
            bool onlyTestsAllowedToBeManuallyExecuted = false,
            Func<TestDefinition, bool> testFilter = null)
            where TAccessRolesEnum : Enum
            => DiscoverTestDefinitions(includeInvalidTests, onlyTestsAllowedToBeManuallyExecuted, userRoles, testFilter);

        /// <summary>
        /// Discover tests.
        /// </summary>
        public List<TestClassDefinition> DiscoverTestDefinitions(
            bool includeInvalidTests = false,
            bool onlyTestsAllowedToBeManuallyExecuted = false,
            object userRolesEnum = null,
            Func<TestDefinition, bool> testFilter = null)
        {
            var assemblies = AssembliesContainingTests;
            if (assemblies == null || !assemblies.Any())
            {
                throw new ArgumentException(
                    $"{nameof(AssembliesContainingTests)} must contain at least one assembly to retrieve tests from.");
            }

            var testClassTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<RuntimeTestClassAttribute>(inherit: true) != null)
                .ToList();

            var testDefinitions = new List<TestClassDefinition>();
            foreach (var classType in testClassTypes)
            {
                var testClassAttribute = classType.GetCustomAttribute<RuntimeTestClassAttribute>(inherit: true);
                var classDef = new TestClassDefinition(classType, testClassAttribute);
                var methods = classType.GetMethods();

                foreach(var testMethod in methods)
                {
                    var testAttribute = testMethod.GetCustomAttribute<RuntimeTestAttribute>();
                    var proxyTestAttribute = testMethod.GetCustomAttribute<ProxyRuntimeTestsAttribute>();

                    // Normal tests
                    if (testAttribute != null)
                    {
                        var testDef = new TestDefinition(testMethod, testAttribute, classDef, ReferenceParameterFactories);

                        bool includeTest = ShouldIncludeTest(includeInvalidTests, onlyTestsAllowedToBeManuallyExecuted, userRolesEnum, testDef);
                        if (includeTest && testFilter?.Invoke(testDef) != false)
                        {
                            classDef.Tests.Add(testDef);
                        }
                    }
                    // Proxy class tests
                    else if (proxyTestAttribute != null && IncludeProxyTests)
                    {
                        // Check for load errors
                        var errors = ValidateProxyTestMethod(classType, testMethod);
                        if (errors.Any())
                        {
                            var testDef = new TestDefinition(testMethod, classDef, ReferenceParameterFactories)
                            {
                                LoadErrors = errors
                            };
                            classDef.Tests.Add(testDef);
                            continue;
                        }

                        var config = testMethod.Invoke(null, new object[0]) as ProxyRuntimeTestConfig;
                        var proxyMethods = config.TargetClassType.GetMethods()
                            .Where(t => !_excludedProxyMethodNames.Contains(t.Name) && !t.IsSpecialName && t.IsPublic && !t.IsStatic && !t.IsGenericMethod)
                            .Where(m => !m.GetCustomAttributes(typeof(CompilerGeneratedAttribute), true).Any());
                        foreach (var proxyMethod in proxyMethods)
                        {
                            var testDef = new TestDefinition(proxyMethod, proxyTestAttribute, config, classDef, ReferenceParameterFactories);
                            bool includeTest = ShouldIncludeTest(includeInvalidTests, onlyTestsAllowedToBeManuallyExecuted, userRolesEnum, testDef);
                            if (includeTest && testFilter?.Invoke(testDef) != false)
                            {
                                classDef.Tests.Add(testDef);
                            }
                        }
                    }
                }

                // Only include test set if it has any tests
                if (classDef.Tests.Any())
                {
                    testDefinitions.Add(classDef);
                }
            }

            return testDefinitions;
        }

        private readonly string[] _excludedProxyMethodNames = typeof(object).GetMethods().Select(x => x.Name).ToArray();

        private static List<string> ValidateProxyTestMethod(Type classType, MethodInfo testMethod)
        {
            var errors = new List<string>();
            if (!testMethod.IsStatic)
            {
                errors.Add($"Test method '{classType.Name}.{testMethod.Name}' must be static.");
            }

            if (testMethod.ReturnType != typeof(ProxyRuntimeTestConfig))
            {
                errors.Add($"Test method '{classType.Name}.{testMethod.Name}' must return a {nameof(ProxyRuntimeTestConfig)}.");
            }

            if (testMethod.GetParameters().Length > 0)
            {
                errors.Add($"Test method '{classType.Name}.{testMethod.Name}' cannot have any parameters.");
            }
            return errors;
        }

        private bool ShouldIncludeTest(bool includeInvalidTests, bool onlyTestsAllowedToBeManuallyExecuted, object userRolesEnum, TestDefinition testDef)
        {
            // Check for invalid tests
            if (!includeInvalidTests && !testDef.Validate().IsValid)
            {
                return false;
            }
            // Check for tests not allowed to be executed manually
            else if (onlyTestsAllowedToBeManuallyExecuted && !testDef.AllowManualExecution)
            {
                return false;
            }
            // Exclude tests that are outside the given roles if any
            else if (!IsTestIncludedForRoles(testDef, userRolesEnum))
            {
                return false;
            }

            return true;
        }

        private bool IsTestIncludedForRoles(TestDefinition test, object roles)
        {
            // No access set => allow
            if (roles == null || test.RolesWithAccess == null)
            {
                return true;
            }
            
            // Check for invalid setup
            if (!EnumUtils.IsEnumFlagOfType(test.RolesWithAccess, new[] { typeof(int), typeof(byte) }))
            {
                throw new InvalidAccessRolesDefinitionException($"Access role set on test '{test.Name}' is either missing a [Flags] attribute or does not have the underlying type int or byte.");
            }
            else if(roles.GetType() != test.RolesWithAccess.GetType())
            {
                throw new InvalidAccessRolesDefinitionException($"Different access role types used on '{test.Name}' and in the discover tests call. " +
                    $"({test.RolesWithAccess.GetType().Name} and {roles.GetType().Name})");
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(roles, test.RolesWithAccess);
        }
    }
}
