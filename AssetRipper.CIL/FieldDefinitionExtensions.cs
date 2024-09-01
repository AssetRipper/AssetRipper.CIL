using AsmResolver.DotNet;

namespace AssetRipper.CIL;

public static class FieldDefinitionExtensions
{
	public static bool IsInstance(this FieldDefinition field)
	{
		return !field.IsStatic;
	}
}
