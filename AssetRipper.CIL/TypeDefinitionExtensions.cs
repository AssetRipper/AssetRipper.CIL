using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL;

public static class TypeDefinitionExtensions
{
	public static FieldDefinition AddField(this TypeDefinition type, string? fieldName, TypeSignature fieldType, bool isStatic = false, Visibility visibility = Visibility.Public)
	{
		FieldAttributes fieldAttributes = visibility.ToFieldAttributes() | (isStatic ? FieldAttributes.Static : default);
		FieldDefinition field = new FieldDefinition(fieldName, fieldAttributes, fieldType);
		type.Fields.Add(field);
		return field;
	}

	public static FieldDefinition AddConstantField(
		this TypeDefinition type,
		TypeSignature fieldType,
		string fieldName,
		Constant constant,
		Visibility visibility = Visibility.Public)
	{
		FieldDefinition field = new FieldDefinition(
			fieldName,
			FieldAttributes.Static |
			FieldAttributes.Literal |
			FieldAttributes.HasDefault |
			visibility.ToFieldAttributes(),
			new FieldSignature(fieldType));
		field.Constant = constant;
		type.Fields.Add(field);
		return field;
	}

	public static MethodDefinition AddMethod(this TypeDefinition type, string? methodName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		MethodDefinition method = CreateMethod(methodName, methodAttributes, returnType);
		type.Methods.Add(method);
		return method;
	}

	public static MethodDefinition AddConstructor(this TypeDefinition type, Visibility visibility = Visibility.Public)
	{
		MethodAttributes methodAttributes = visibility.ToMethodAttributes() | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName;
		return type.AddMethod(".ctor", methodAttributes, type.DeclaringModule!.CorLibTypeFactory.Void);
	}

	public static FieldDefinition AddEnumField(this TypeDefinition type, string name, long value)
	{
		TypeSignature underlyingType = type.GetEnumUnderlyingType() ?? throw new ArgumentException("Type is not an enum", nameof(type));

		return type.AddConstantField(type.ToTypeSignature(), name, FromValue(value, underlyingType.ElementType));

		static Constant FromValue(long value, ElementType elementType) => elementType switch
		{
			ElementType.I1 => Constant.FromValue((sbyte)value),
			ElementType.U1 => Constant.FromValue((byte)value),
			ElementType.I2 => Constant.FromValue((short)value),
			ElementType.U2 => Constant.FromValue((ushort)value),
			ElementType.I4 => Constant.FromValue((int)value),
			ElementType.U4 => Constant.FromValue((uint)value),
			ElementType.I8 => Constant.FromValue(value),
			ElementType.U8 => Constant.FromValue(unchecked((ulong)value)),
			_ => throw new ArgumentOutOfRangeException(nameof(elementType)),
		};
	}

	private static MethodDefinition CreateMethod(string? methodName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
		MethodSignature methodSignature = isStatic
			? MethodSignature.CreateStatic(returnType)
			: MethodSignature.CreateInstance(returnType);
		MethodDefinition result = new MethodDefinition(methodName, methodAttributes, methodSignature);

		result.CilMethodBody = new CilMethodBody();

		return result;
	}

	/// <summary>
	/// Gets the default constructor for a <see cref="TypeDefinition"/>. Throws an exception if one doesn't exist.
	/// </summary>
	public static MethodDefinition GetDefaultConstructor(this TypeDefinition type)
	{
		return type.Methods.First(m => m.IsInstanceConstructor() && m.Parameters.Count == 0);
	}

	/// <summary>
	/// Get all the instance constructors for a type
	/// </summary>
	/// <param name="type">The definition for the type</param>
	/// <returns>All the instance constructors</returns>
	public static IEnumerable<MethodDefinition> GetInstanceConstructors(this TypeDefinition type)
	{
		return type.Methods.Where(m => !m.IsStatic && m.IsConstructor);
	}

	public static bool IsStatic(this TypeDefinition type)
	{
		return type.IsSealed && type.IsAbstract;
	}
}
