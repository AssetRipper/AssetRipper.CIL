using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

	public static TypeDefinition AddEmptyEnum(this ModuleDefinition module, string? @namespace, string? name, Visibility visibility, ElementType underlyingType)
	{
		CorLibTypeSignature underlyingTypeSignature = module.CorLibTypeFactory.FromElementType(underlyingType)
			?? throw new ArgumentOutOfRangeException(nameof(underlyingType), underlyingType, null);

		TypeReference baseType = new(module.CorLibTypeFactory.CorLibScope, "System", "Enum");
		TypeDefinition type = new(@namespace, name, visibility.ToTypeAttributes() | TypeAttributes.Sealed, baseType);
		module.TopLevelTypes.Add(type);
		AddEnumValue(type, underlyingTypeSignature);
		return type;

		static void AddEnumValue(TypeDefinition type, TypeSignature underlyingType)
		{
			FieldSignature fieldSignature = new FieldSignature(underlyingType);
			FieldDefinition fieldDef = new FieldDefinition("value__", FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RuntimeSpecialName, fieldSignature);
			type.Fields.Add(fieldDef);
		}
	}

	public static TypeDefinition AddExistingEnum<T>(this ModuleDefinition module) where T : struct, Enum
	{
		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
		bool isSigned = underlyingType == typeof(sbyte) || underlyingType == typeof(short) || underlyingType == typeof(int) || underlyingType == typeof(long);
		CorLibTypeSignature underlyingTypeSignature = module.CorLibTypeFactory.FromType(underlyingType)
			?? throw new InvalidOperationException($"Failed to get underlying type for {typeof(T)}");

		TypeDefinition type = module.AddEmptyEnum(typeof(T).Namespace, typeof(T).Name, VisibilityExtensions.GetVisibility(typeof(T)), underlyingTypeSignature.ElementType);
		foreach (T value in GetValues())
		{
			type.AddEnumField(value.ToString(), ToInt64(value));
		}

		if (typeof(T).CustomAttributes.Any(a => a.AttributeType is { Namespace: "System", Name: nameof(FlagsAttribute) }))
		{
			type.AddFlagsAttribute();
		}

		return type;

		static IEnumerable<T> GetValues()
		{
#if NETSTANDARD
			return Enum.GetValues(typeof(T)).Cast<T>();
#else
			return Enum.GetValues<T>();
#endif
		}

		long ToInt64(T value)
		{
			if (Unsafe.SizeOf<T>() == sizeof(long))
			{
				return Unsafe.As<T, long>(ref value);
			}
			else if (Unsafe.SizeOf<T>() == sizeof(int))
			{
				return isSigned ? Unsafe.As<T, int>(ref value) : Unsafe.As<T, uint>(ref value);
			}
			else if (Unsafe.SizeOf<T>() == sizeof(short))
			{
				return isSigned ? Unsafe.As<T, short>(ref value) : Unsafe.As<T, ushort>(ref value);
			}
			else if (Unsafe.SizeOf<T>() == sizeof(byte))
			{
				return isSigned ? Unsafe.As<T, sbyte>(ref value) : Unsafe.As<T, byte>(ref value);
			}
			else
			{
				throw new NotSupportedException($"Unsupported enum underlying type {typeof(T)}");
			}
		}
	}
}
