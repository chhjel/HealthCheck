using HealthCheck.Core.Attributes;
using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Extensions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HealthCheck.Core.Entities
{
    /// <summary>
    /// A definition of a runtime test. Extracted from a method decorated with <see cref="RuntimeTestAttribute"/>.
    /// </summary>
    public class TestDefinition
    {
        /// <summary>
        /// Unique id for this test.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Name of the test. Defaults to the name of the method if no custom name is provided.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the test.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Test parameters.
        /// </summary>
        public TestParameter[] Parameters { get; private set; }

        /// <summary>
        /// Optional value to override default option on the test class.
        /// </summary>
        public bool? AllowParallelExecution { get; set; }

        ///// <summary>
        ///// Optional boolean value to override config for this test. If enabled the test can be executed from the ui manually.
        ///// </summary>
        //public object AllowManualExecution { get; set; }

        /// <summary>
        /// Test method.
        /// </summary>
        internal MethodInfo Method { get; private set; }

        /// <summary>
        /// Parent class definition;
        /// </summary>
        public TestClassDefinition ParentClass { get; private set; }

        /// <summary>
        /// Create a new <see cref="TestDefinition"/>.
        /// </summary>
        public TestDefinition(MethodInfo method, RuntimeTestAttribute testAttribute, TestClassDefinition parentClass)
        {
            Method = method;
            ParentClass = parentClass;

            Name = testAttribute.Name ?? Method.Name.AddSpaceBetweenCapitalLetters();
            Description = testAttribute.Description;
            AllowParallelExecution = (testAttribute.AllowParallelExecution is bool allowParallelExecution && allowParallelExecution);

            SetId();
            InitParameters(testAttribute);
        }

        private void SetId()
        {
            var methodParametersSignature = string.Join("-", Method.GetParameters().Select(x => x.ParameterType.Name));
            Id = $"{ParentClass.ClassType.FullName}.{Method.Name}.{methodParametersSignature}";
        }

        private void InitParameters(RuntimeTestAttribute testAttribute)
        {
            var methodParameters = Method.GetParameters();
            Parameters = new TestParameter[methodParameters.Length];
            for(int i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];
                Parameters[i] = new TestParameter()
                {
                    Index = i,
                    Name = testAttribute.GetCustomParameterName(i) ?? parameter.Name.AddSpaceBetweenCapitalLetters(),
                    Description = testAttribute.GetCustomParameterDescription(i),
                    DefaultValue = parameter.DefaultValue,
                    ParameterType = parameter.ParameterType
                };
            }
        }

        /// <summary>
        /// Run the test.
        /// </summary>
        public async Task<TestResult> ExecuteTest(object instance, object[] parameters)
        {
            var methodParams = Method.GetParameters();
            var parameterCount = methodParams.Length;
            var parameterList = new object[parameterCount];
            for (int i = 0; i < parameterCount; i++)
            {
                var value = (parameters != null && parameters.Length >= i - 1)
                    ? parameters[i]
                    : null;

                if (value == null && methodParams[i].DefaultValue != null)
                {
                    value = methodParams[i].DefaultValue;
                }

                parameterList[i] = value;
            }

            if (Method.ReturnType == typeof(TestResult))
            {
                return (TestResult)Method.Invoke(instance, parameterList);
            }
            else if (Method.ReturnType == typeof(Task<TestResult>))
            {
                var task = (Task<TestResult>)Method.Invoke(instance, parameterList);
                return await task;
            }
            else
            {
                throw new InvalidTestDefinitionException($"Method {ParentClass.ClassType.Name}.{Method.Name} does not return a TestResult or Task<TestResult>.");
            }
        }

        /// <summary>
        /// Validates the test method.
        /// </summary>
        public TestDefinitionValidationResult Validate()
        {
            var result = new TestDefinitionValidationResult(this);

            if (Method.ReturnType != typeof(TestResult) && Method.ReturnType != typeof(Task<TestResult>))
                result.Error = $"Test method '{ParentClass.ClassType.Name}.{Method.Name}' must return a TestResult or Task<TestResult>.";
            else if (Method.GetParameters().Any(x => !x.HasDefaultValue))
                result.Error = $"Test method '{ParentClass.ClassType.Name}.{Method.Name}' must have default values for all parameters.";

            return result;
        }

        /// <summary>
        /// Test id.
        /// </summary>
        public override string ToString() => Id;
    }
}
