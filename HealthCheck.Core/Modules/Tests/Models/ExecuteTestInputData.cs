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
        public List<string> Parameters { get; set; }

        /// <summary>
        /// Convert the parameter values as the given types using the given string converter.
        /// </summary>
        public object[] GetParametersWithConvertedTypes(Type[] types, StringConverter stringConverter, TestDefinition test)
        {
            var objects = new object[types.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                var parameter = test.Parameters[i];
                var inputStringValue = Parameters[i];
                var type = types[i];

                object convertedObject = null;
                if (parameter.IsCustomReferenceType)
                {
                    if (!string.IsNullOrWhiteSpace(inputStringValue)
                        && test?.ClassProxyConfig?.GetFactoryForType(type) != null)
                    {
                        var parameterFactoryData = test.ClassProxyConfig.GetFactoryForType(type);
                        convertedObject = parameterFactoryData.GetInstanceByIdFor(type, inputStringValue);
                    }
                    else if (!string.IsNullOrWhiteSpace(inputStringValue)
                       && parameter.ReferenceFactory?.CanFactorizeFor(type) != null)
                    {
                        convertedObject = parameter.ReferenceFactory?.GetInstanceByIdFor(type, inputStringValue);
                    }
                }
                else
                {
                    convertedObject = stringConverter.ConvertStringTo(type, inputStringValue);
                }
                objects[i] = convertedObject;
            }
            return objects;
        }
    }
}
