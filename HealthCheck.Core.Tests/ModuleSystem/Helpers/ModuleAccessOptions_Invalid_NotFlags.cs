namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
    public enum ModuleAccessOptions_Invalid_NotFlags
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
        DeleteEverything = 8
    }
}
