using HealthCheck.Core.Attributes;
using HealthCheck.Core.Extensions;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// A definition of a runtime test class. Extracted from a class decorated with <see cref="RuntimeTestClassAttribute"/>.
    /// </summary>
    public class TestClassDefinition
    {
        /// <summary>
        /// Type of the class.
        /// </summary>
        public Type ClassType { get; private set; }

        /// <summary>
        /// Test set id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the test set.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test set.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// If true the test in this class will be executed in parallel by default.
        /// </summary>
        public bool AllowParallelExecution { get; set; }

        /// <summary>
        /// Test methods defined within this class.
        /// </summary>
        public List<TestDefinition> Tests { get; set; } = new List<TestDefinition>();

        /// <summary>
        /// Initialize a new <see cref="TestClassDefinition"/>.
        /// </summary>
        public TestClassDefinition(Type classType, RuntimeTestClassAttribute testClassAttribute)
        {
            ClassType = classType;
            Id = testClassAttribute.Id ?? ClassType.FullName;
            Name = testClassAttribute.Name ?? ClassType.Name.AddSpaceBetweenCapitalLetters();
            Description = testClassAttribute.Description;
            AllowParallelExecution = testClassAttribute.AllowParallelExecution;
        }
    }
}
