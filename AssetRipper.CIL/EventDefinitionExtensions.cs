using AsmResolver.DotNet;

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
}
