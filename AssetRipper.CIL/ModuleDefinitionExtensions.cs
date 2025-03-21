using AsmResolver.DotNet;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class ModuleDefinitionExtensions
{
	public static bool TryGetTopLevelType(this ModuleDefinition module, string? @namespace, string? name, [NotNullWhen(true)] out TypeDefinition? type)
	{
		type = module.TopLevelTypes.FirstOrDefault(t => t.Namespace == @namespace && t.Name == name);
		return type != null;
	}

	public static bool HasTopLevelType(this ModuleDefinition module, string? @namespace, string? name)
	{
		return module.TryGetTopLevelType(@namespace, name, out _);
	}
}
