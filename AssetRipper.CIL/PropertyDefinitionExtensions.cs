using AsmResolver.DotNet;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class PropertyDefinitionExtensions
{
	public static bool IsAutoProperty(this PropertyDefinition property, [NotNullWhen(true)] out FieldDefinition? backingField)
	{
		backingField = null;
		return (property.GetMethod is { } getMethod && getMethod.IsAutoPropertyGetMethod(out backingField))
			|| (property.SetMethod is { } setMethod && setMethod.IsAutoPropertySetMethod(out backingField));
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
