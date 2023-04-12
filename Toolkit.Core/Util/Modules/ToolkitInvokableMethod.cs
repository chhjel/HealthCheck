using QoDL.Toolkit.Core.Abstractions;
using QoDL.Toolkit.Core.Abstractions.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QoDL.Toolkit.Core.Util.Modules;

/// <summary>
/// A method on a module that can be invoked from frontend.
/// </summary>
public class ToolkitInvokableMethod
{
    /// <summary>
    /// Name of the method.
    /// </summary>
    public string Name => Method.Name;

    /// <summary>
    /// True if the method returns a task.
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Enum values required to invoke this method.
    /// </summary>
    public object RequiresAccessTo { get; set; }

    /// <summary>
    /// Type of the parameter.
    /// </summary>
    public Type ParameterType { get; set; }

    /// <summary>
    /// True if there is any parameters.
    /// </summary>
    public bool HasParameterType => ParameterType != null;

    /// <summary>
    /// True if the first parameter is a <see cref="ToolkitModuleContext"/>.
    /// </summary>
    public bool HasContextParameter { get; set; }

    /// <summary>
    /// Return type from the method.
    /// </summary>
    public Type ReturnType { get; set; }

    /// <summary>
    /// True if the method is not void or returns an empty Task.
    /// </summary>
    public bool HasReturnType => ReturnType != null;

    private MethodInfo Method { get; set; }

    /// <summary>
    /// A method on a module that can be invoked from frontend.
    /// </summary>
    public ToolkitInvokableMethod(MethodInfo method)
    {
        Method = method;
        var attribute = method.GetCustomAttributes(true)
            .FirstOrDefault(a => (a is ToolkitModuleMethodAttribute))
            as ToolkitModuleMethodAttribute;

        RequiresAccessTo = attribute.RequiresAccessTo;

        IsAsync = MethodIsAsync(method);
        ReturnType = GetReturnType(method);

        var parameters = method.GetParameters();
        if (parameters.Length > 0)
        {
            HasContextParameter = parameters[0].ParameterType == typeof(ToolkitModuleContext);
            if (parameters.Length > 1 || !HasContextParameter)
            {
                ParameterType = parameters.Last().ParameterType;
            }
        }
    }

    /// <summary>
    /// Invoke the method with the given serialized parameter. Returns a serialized response.
    /// </summary>
    public async Task<string> Invoke(IToolkitModule instance, ToolkitModuleContext context, string jsonPayload, IJsonSerializer serializer)
    {
        List<object> parameters = new();
        if (HasContextParameter)
        {
            parameters.Add(context);
        }
        if (HasParameterType)
        {
            parameters.Add(serializer.Deserialize(jsonPayload, ParameterType));
        }

        var result = Method.Invoke(instance, (parameters.Count == 0) ? null : parameters.ToArray());
        if (result is Task resultTask)
        {
            await resultTask.ConfigureAwait(false);

            var resultProperty = resultTask.GetType().GetProperty("Result");
            result = resultProperty.GetValue(resultTask);
        }

        return (HasReturnType) ? serializer.Serialize(result) : null;
    }

    private static Type GetReturnType(MethodInfo method)
    {
        var type = (method.ReturnType == typeof(void) || method.ReturnType == typeof(Task)) ? null : method.ReturnType;
        if (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments()[0];
        }
        return type;
    }

    private static bool MethodIsAsync(MethodInfo method)
    {
        var returnType = method.ReturnType;
        if (returnType == typeof(Task))
        {
            return true;
        }
        else if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            return true;
        }

        return false;
    }
}
