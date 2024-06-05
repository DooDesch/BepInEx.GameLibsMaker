using Mono.Cecil;

namespace EnoPM.BepInEx.GameLibsMaker.Extensions;

internal static class FieldDefinitionExtensions
{
    internal static void Publicize(this FieldDefinition field)
    {
        if (field.IsPublic) return;
        field.IsPrivate = field.IsAssembly = field.IsFamily = false;
        field.IsPublic = true;
    }
}