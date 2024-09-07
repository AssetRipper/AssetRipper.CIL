using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class EventDefinitionExtensions
{
	public static bool IsInstance(this EventDefinition eventDefinition)
	{
		return (eventDefinition.AddMethod?.IsInstance() ?? false)
			|| (eventDefinition.RemoveMethod?.IsInstance() ?? false);
	}

	public static bool IsStatic(this EventDefinition eventDefinition)
	{
		return (eventDefinition.AddMethod?.IsStatic ?? false)
			|| (eventDefinition.RemoveMethod?.IsStatic ?? false);
	}

	public static bool HasAddMethod(this EventDefinition eventDefinition)
	{
		return eventDefinition.AddMethod is not null;
	}

	public static bool HasRemoveMethod(this EventDefinition eventDefinition)
	{
		return eventDefinition.RemoveMethod is not null;
	}

	public static bool HasFireMethod(this EventDefinition eventDefinition)
	{
		return eventDefinition.FireMethod is not null;
	}

	public static bool HasBackingField(this EventDefinition eventDefinition, [NotNullWhen(true)] out FieldDefinition? backingField)
	{
		TypeSignature? eventType = eventDefinition.EventType?.ToTypeSignature();
		if (eventType is not null)
		{
			foreach (FieldDefinition field in eventDefinition.DeclaringType?.Fields ?? [])
			{
				if (field.IsPrivate && field.Name == eventDefinition.Name && SignatureComparer.Default.Equals(field.Signature?.FieldType, eventType))
				{
					backingField = field;
					return true;
				}
			}
		}

		backingField = null;
		return false;
	}
}
