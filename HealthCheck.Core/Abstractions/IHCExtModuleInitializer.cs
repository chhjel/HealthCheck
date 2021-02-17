namespace HealthCheck.Core.Abstractions
{
    /// <summary>
    /// Used to initialize data in external module assemblies.
    /// </summary>
    public interface IHCExtModuleInitializer
    {
        /// <summary>
        /// Invoked on HC startup.
        /// </summary>
        void Initialize();
    }
}
