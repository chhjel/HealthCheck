namespace QoDL.Toolkit.Core.Abstractions
{
    /// <summary>
    /// Used to initialize data in external module assemblies.
    /// </summary>
    public interface ITKExtModuleInitializer
    {
        /// <summary>
        /// Invoked on TK startup.
        /// </summary>
        void Initialize();
    }
}
