using HealthCheck.Core.Attributes;
using HealthCheck.Core.Entities;
using HealthCheck.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HealthCheck.Core.TestManagers
{
    /// <summary>
    /// Discovers and validates test methods and test sets.
    /// </summary>
    public class TestDiscoverer
    {
        /// <summary>
        /// The assembly that contains the test methods. Defaults to entry assembly.
        /// </summary>
        public Assembly AssemblyContainingTests { get; set; }

        /// <summary>
        /// Create a new <see cref="TestDiscoverer"/>.
        /// </summary>
        public TestDiscoverer() {}

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
        /// Discover tests.
        /// </summary>
        public List<TestClassDefinition> DiscoverTestDefinitions(bool includeInvalidTests = false)
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
                // todo check that we have empty constructor
                //var constructor = classType.GetConstructor(new Type[0]);

                var testClassAttribute = classType.GetCustomAttribute<RuntimeTestClassAttribute>(inherit: true);
                var classDef = new TestClassDefinition(classType, testClassAttribute);
                var testMethods = classType.GetMethods()
                    .Where(x => x.GetCustomAttribute<RuntimeTestAttribute>(inherit: true) != null)
                    .ToList();

                foreach(var testMethod in testMethods)
                {
                    var testAttribute = testMethod.GetCustomAttribute<RuntimeTestAttribute>();
                    var testDef = new TestDefinition(testMethod, testAttribute, classDef);

                    if (!includeInvalidTests && !testDef.Validate().IsValid)
                    {
                        continue;
                    }

                    classDef.Tests.Add(testDef);
                }
                testDefinitions.Add(classDef);
            }
            return testDefinitions;
        }

        // Validate
    }
}
