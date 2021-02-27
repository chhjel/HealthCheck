using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;
using System;
using System.Collections.Generic;

namespace HealthCheck.Core.Modules.Tests.Models
{
    /// <summary>
    /// Input data type for the execute test endpoint.
    /// </summary>
    public class ExecuteTestInputData
    {
        /// <summary>
        /// Id of the test to execute.
        /// </summary>
        public string TestId { get; set; }

        /// <summary>
        /// Stringified parameter values.
        /// </summary>
        public List<ExecuteTestParameterInputData> Parameters { get; set; }

        /// <summary>
        /// Convert the parameter values as the given types using the given string converter.
        /// </summary>
        public object[] GetParametersWithConvertedTypes(Type[] types, StringConverter stringConverter, TestDefinition test)
        {
            var objects = new object[types.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                var parameter = test.Parameters[i];
                var inputData = Parameters[i];
                var inputValue = inputData.Value;
                var type = types[i];

                object convertedObject = null;
                if (inputData.IsUnsupportedJson)
                {
                    convertedObject = TestRunnerService.Serializer?.Deserialize(inputValue, type);
                    var error = TestRunnerService.Serializer?.LastError;
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        throw new HCException(error);
                    }
                }
                else if (parameter.IsCustomReferenceType && !string.IsNullOrWhiteSpace(inputValue))
                {
                    var factory = parameter.GetParameterFactory(test);
                    convertedObject = factory?.GetInstanceByIdFor(type, inputValue);
                }
                else if (!parameter.IsCustomReferenceType)
                {
                    convertedObject = stringConverter.ConvertStringTo(type, inputValue);
                }
                objects[i] = convertedObject;
            }
            return objects;
        }
    }

    /// <summary>
    /// Input data type for the execute test endpoint.
    /// </summary>
    public class ExecuteTestParameterInputData
    {
        /// <summary>
        /// Serialized input value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// True if parameter is of unknown type.
        /// </summary>
        public bool IsUnsupportedJson { get; set; }
    }
}
