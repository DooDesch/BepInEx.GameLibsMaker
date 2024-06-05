using Mono.Cecil;

namespace EnoPM.BepInEx.GameLibsMaker.Extensions;

internal static class TypeDefinitionExtensions
{
    internal static void Publicize(this TypeDefinition type)
    {
        if (!type.IsPublic)
        {
            type.IsPublic = true;
        }
        foreach (var method in type.Methods)
        {
            method.Publicize();
        }
        foreach (var field in type.Fields)
        {
            field.Publicize();
        }
        foreach (var nestedType in type.NestedTypes)
        {
            nestedType.Publicize();
        }
    }

    internal static void Strip(this TypeDefinition type, MethodReference notImplementedConstructor)
    {
        foreach (var method in type.Methods)
        {
            method.Strip(notImplementedConstructor);
        }
        foreach (var nestedType in type.NestedTypes)
        {
            nestedType.Strip(notImplementedConstructor);
        }
    }
}