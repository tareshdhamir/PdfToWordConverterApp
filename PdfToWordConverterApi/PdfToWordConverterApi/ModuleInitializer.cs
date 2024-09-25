using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public static class ModuleInitializer
{
    [ModuleInitializer]
    [ExcludeFromCodeCoverage]
    public static void Initialize()
    {
        // This code is part of the top-level statements and can be excluded
    }
}
