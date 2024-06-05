using System.Reflection;
using Mono.Cecil;

namespace EnoPM.BepInEx.GameLibsMaker.Extensions;

internal static class ModuleDefinitionExtensions
{
    private static readonly ConstructorInfo NotImplementedConstructor;

    static ModuleDefinitionExtensions()
    {
        NotImplementedConstructor = typeof(NotImplementedException).GetConstructor(Type.EmptyTypes)!;
    }
    
    internal static void Publicize(this ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            type.Publicize();
        }
    }
    
    internal static void Strip(this ModuleDefinition module)
    {
        var notImplementedConstructor = module.ImportReference(NotImplementedConstructor);
        foreach (var type in module.Types)
        {
            type.Strip(notImplementedConstructor);
        }
    }
}