namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Serializes data dumps. Should ignore errors and indent result json.
    /// </summary>
    public interface IDumpJsonSerializer
    {
        /// <summary>
        /// Serialize the given object into json. Should be indented.
        /// </summary>
        string Serialize(object obj);
    }
}
