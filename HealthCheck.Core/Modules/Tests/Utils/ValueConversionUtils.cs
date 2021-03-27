using HealthCheck.Core.Exceptions;
using HealthCheck.Core.Modules.Tests.Services;
using HealthCheck.Core.Util;

namespace HealthCheck.Core.Modules.Tests.Utils
{
    /// <summary>
    /// Conversion utils.
    /// </summary>
    public static class HCValueConversionUtils
    {
        /// <summary>
        /// Attempt to convert the given input to an instance of the given type.
        /// </summary>
        public static object ConvertInput(HCValueInput input, StringConverter stringConverter)
        {
            object convertedObject = null;
            if (input.IsJson)
            {
                convertedObject = TestRunnerService.Serializer?.Deserialize(input.Value, input.Type);
                var error = TestRunnerService.Serializer?.LastError;
                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new HCException(error);
                }
            }
            else if (input.IsCustomReferenceType && !string.IsNullOrWhiteSpace(input.Value))
            {
                var factory = input.ParameterFactoryFactory();
                convertedObject = factory?.GetInstanceByIdFor(input.Type, input.Value);
            }
            else if (!input.IsCustomReferenceType)
            {
                convertedObject = stringConverter.ConvertStringTo(input.Type, input.Value);
            }
            return convertedObject;
        }
    }
}
