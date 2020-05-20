using HealthCheck.Core.Attributes;
using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Attributes;
using HealthCheck.Core.Modules.Tests.Models;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.Modules.Tests.Services
{
    /// <summary>
    /// Discovers and validates test methods and test sets.
    /// </summary>
    public class TestDiscoveryService
    {
        /// <summary>
        /// The assembly that contains the test methods. Defaults to entry assembly.
        /// </summary>
        public Assembly AssemblyContainingTests { get; set; }

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

        private Assembly GetAssembly()
            => AssemblyContainingTests ?? Assembly.GetEntryAssembly();

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
            var assembly = GetAssembly();
            if (assembly == null)
            {
                throw new ArgumentNullException($"Could not find entry assembly, set {nameof(AssemblyContainingTests)} to the assembly that contains the tests.");
            }

            // todo cache types? Or instances? option in attribute?
            var testClassTypes = assembly.GetTypes()
                .Where(x => x.GetCustomAttribute<RuntimeTestClassAttribute>(inherit: true) != null)
                .ToList();

            var testDefinitions = new List<TestClassDefinition>();
            foreach (var classType in testClassTypes)
            {
                var testClassAttribute = classType.GetCustomAttribute<RuntimeTestClassAttribute>(inherit: true);
                var classDef = new TestClassDefinition(classType, testClassAttribute);
                var testMethods = classType.GetMethods()
                    .Where(x => x.GetCustomAttribute<RuntimeTestAttribute>(inherit: true) != null)
                    .ToList();

                foreach(var testMethod in testMethods)
                {
                    var testAttribute = testMethod.GetCustomAttribute<RuntimeTestAttribute>();
                    var testDef = new TestDefinition(testMethod, testAttribute, classDef);

                    // Check for invalid tests
                    if (!includeInvalidTests && !testDef.Validate().IsValid)
                    {
                        continue;
                    }

                    // Check for tests not allowed to be executed manually
                    if (onlyTestsAllowedToBeManuallyExecuted && !testDef.AllowManualExecution)
                    {
                        continue;
                    }

                    // Exclude tests that are outside the given roles if any
                    if (!IsTestIncludedForRoles(testDef, userRolesEnum))
                    {
                        continue;
                    }

                    if (testFilter?.Invoke(testDef) != false)
                    {
                        classDef.Tests.Add(testDef);
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
            else if(roles != null && roles.GetType() != test.RolesWithAccess.GetType())
            {
                throw new InvalidAccessRolesDefinitionException($"Different access role types used on '{test.Name}' and in the discover tests call. " +
                    $"({test.RolesWithAccess.GetType().Name} and {roles.GetType().Name})");
            }
            
            // Test requires roles, but user has none => don't allow.
            if (roles == null)
            {
                return false;
            }

            return EnumUtils.EnumFlagHasAnyFlagsSet(roles, test.RolesWithAccess);
        }
    }
}
