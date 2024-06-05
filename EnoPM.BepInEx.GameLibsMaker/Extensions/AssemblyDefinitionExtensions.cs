using Mono.Cecil;

namespace EnoPM.BepInEx.GameLibsMaker.Extensions;

internal static class AssemblyDefinitionExtensions
{
    internal static void Publicize(this AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
        {
            module.Publicize();
        }
    }
    
    internal static void Strip(this AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
        {
            module.Strip();
        }
    }
}