using Mono.Cecil;

namespace TheOtherRoles.AmongUsGameLibs.Maker.Extensions;

public static class AssemblyDefinitionExtensions
{
    public static TypeDefinition FindTypeInReferences(this AssemblyDefinition assembly, string typeName)
    {
        foreach (var module in assembly.Modules)
        {
            foreach (var type in module.Types)
            {
                if (type.FullName == typeName)
                {
                    return type;
                }
            }
            foreach (var reference in module.AssemblyReferences)
            {
                try
                {
                    var currentDef = assembly.MainModule.AssemblyResolver.Resolve(reference);
                    var typeDefinition = currentDef.MainModule.Types.FirstOrDefault(x => x.FullName == typeName);
                    if (typeDefinition != null)
                    {
                        return typeDefinition;
                    }
                    var exportedType = currentDef.MainModule.ExportedTypes.FirstOrDefault(x => x.FullName == typeName);
                    if (exportedType != null)
                    {
                        return exportedType.Resolve();
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        if (typeName.StartsWith("System."))
        {
            return FindTypeInReferences(assembly, $"Il2Cpp{typeName}");
        }

        throw new Exception($"Could not find type {typeName} in {assembly.Name} references");
    }
}