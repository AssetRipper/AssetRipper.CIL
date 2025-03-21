using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class FieldDefinitionExtensions
{
	public static bool IsInstance(this FieldDefinition field)
	{
		return !field.IsStatic;
	}

	private static bool IsPropertyBackingField(this FieldDefinition field, [NotNullWhen(true)] out string? propertyName)
	{
		string? name = field.Name;
		if (name is ['<', ..] && name.EndsWith(">k__BackingField", StringComparison.Ordinal))
		{
			propertyName = name[1..^16];
			return true;
		}
		else
		{
			propertyName = null;
			return false;
		}
	}

	public static bool IsPropertyBackingField(this FieldDefinition field, [NotNullWhen(true)] out PropertyDefinition? property)
	{
		if (field.IsPropertyBackingField(out string? propertyName))
		{
			property = field.DeclaringType?.Properties.FirstOrDefault(p => p.Name == propertyName);
			return property is not null
				&& field.IsStatic == property.IsStatic()
				&& SignatureComparer.Default.Equals(field.Signature?.FieldType, property.Signature?.ReturnType);
		}
		else
		{
			property = null;
			return false;
		}
	}

	public static bool IsEventBackingField(this FieldDefinition field, [NotNullWhen(true)] out EventDefinition? @event)
	{
		if (field.IsPrivate)
		{
			@event = field.DeclaringType?.Events.FirstOrDefault(e => e.Name == field.Name);
			return @event is not null
				&& field.IsStatic == @event.IsStatic()
				&& SignatureComparer.Default.Equals(@event.EventType?.ToTypeSignature(), field.Signature?.FieldType);
		}
		else
		{
			@event = null;
			return false;
		}
	}

	public static bool IsFixedBuffer(this FieldDefinition field)
	{
		return field.HasCustomAttribute("System.Runtime.CompilerServices", "FixedBufferAttribute");
	}
}
