using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class PropertyDefinitionExtensions
{
	public static bool HasBackingField(this PropertyDefinition property, [NotNullWhen(true)] out FieldDefinition? backingField)
	{
		string backingFieldName = $"<{property.Name}>k__BackingField";
		backingField = property.DeclaringType?.Fields.FirstOrDefault(f => f.Name == backingFieldName);
		if (backingField is null
			|| !backingField.IsPrivate
			|| backingField.IsStatic != property.IsStatic()
			|| backingField.DeclaringModule?.RuntimeContext is not { } runtimeContext
			|| !runtimeContext.SignatureComparer.Equals(backingField.Signature?.FieldType, property.Signature?.ReturnType))
		{
			backingField = null;
			return false;
		}
		else
		{
			return true;
		}
	}

	public static bool HasGetMethod(this PropertyDefinition property)
	{
		return property.GetMethod is not null;
	}

	public static bool HasSetMethod(this PropertyDefinition property)
	{
		return property.SetMethod is not null;
	}

	public static bool HasBothAccessors(this PropertyDefinition property)
	{
		return property.HasGetMethod() && property.HasSetMethod();
	}

	public static bool IsInstance(this PropertyDefinition property) => property.Signature?.HasThis ?? false;

	public static bool IsStatic(this PropertyDefinition property) => !property.Signature?.HasThis ?? false;
}
