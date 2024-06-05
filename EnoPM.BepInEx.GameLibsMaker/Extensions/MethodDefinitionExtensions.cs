using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace EnoPM.BepInEx.GameLibsMaker.Extensions;

internal static class MethodDefinitionExtensions
{
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: Mono.Cecil.Cil.Instruction; size: 500MB")]
    internal static void Strip(this MethodDefinition method, MethodReference notImplementedConstructor)
    {
        if (method.IsAbstract || !method.HasBody) return;
        method.Body.Instructions.Clear();
        method.Body.Variables.Clear();
        method.Body.ExceptionHandlers.Clear();
        var il = method.Body.GetILProcessor();
        il.Clear();
        if (method.ReturnType.FullName == method.Module.TypeSystem.Void.FullName)
        {
            il.Emit(OpCodes.Ret);
        }
        else
        {
            il.Emit(OpCodes.Newobj, notImplementedConstructor);
            il.Emit(OpCodes.Throw);
        }
    }

    internal static void Publicize(this MethodDefinition method)
    {
        if (method.IsPublic) return;
        method.IsPrivate = method.IsAssembly = method.IsFamily = false;
        method.IsPublic = true;
    }
}