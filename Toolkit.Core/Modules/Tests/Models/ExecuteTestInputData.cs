using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Core.Util.Models;
using System;
using System.Collections.Generic;

namespace QoDL.Toolkit.Core.Modules.Tests.Models;

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
    public object[] GetParametersWithConvertedTypes(Type[] types, TKStringConverter stringConverter, TestDefinition test)
    {
        var objects = new object[types.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            var parameter = test.Parameters[i];
            var inputData = Parameters[i];

            if (parameter.IsOut)
            {
                objects[i] = null;
                continue;
            }

            var conversionInput = new TKValueInput
            {
                Value = inputData.Value,
                Type = types[i],
                IsCustomReferenceType = parameter.IsCustomReferenceType,
                IsJson = inputData.IsUnsupportedJson,
                ParameterFactoryFactory = () => parameter.GetParameterFactory(test)
            };
            objects[i] = TKValueConversionUtils.ConvertInput(conversionInput, stringConverter);
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
