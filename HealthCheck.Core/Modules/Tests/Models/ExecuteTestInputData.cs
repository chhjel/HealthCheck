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
                var type = types[i];
                object convertedObject = null;
                if (test.Type == TestDefinition.TestDefinitionType.ProxyClassMethod && test.ClassProxyConfig.ParameterFactories.ContainsKey(type))
                {
                    if (!string.IsNullOrWhiteSpace(Parameters[i]))
                    {
                        var parameterFactoryData = test.ClassProxyConfig.ParameterFactories[type];
                        convertedObject = parameterFactoryData.GetInstanceFromIdFactory?.Invoke(Parameters[i]);
                    }
                }
                else
                {
                    convertedObject = stringConverter.ConvertStringTo(type, Parameters[i]);
                }
                objects[i] = convertedObject;
            }
            return objects;
        }
    }
}
