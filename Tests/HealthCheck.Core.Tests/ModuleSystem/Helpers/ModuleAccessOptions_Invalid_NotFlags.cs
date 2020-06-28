namespace HealthCheck.Core.Tests.ModuleSystem.Helpers
{
#pragma warning disable S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
    public enum ModuleAccessOptions_Invalid_NotFlags
#pragma warning restore S2344 // Enumeration type names should not have "Flags" or "Enum" suffixes
    {
        None = 0,
        Read = 1,
        Write = 2,
        Execute = 4,
        DeleteEverything = 8
    }
}
