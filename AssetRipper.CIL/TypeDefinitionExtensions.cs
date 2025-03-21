using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL;

public static class TypeDefinitionExtensions
{
	public static FieldDefinition AddField(this TypeDefinition type, string fieldName, TypeSignature fieldType, bool isStatic = false, Visibility visibility = Visibility.Public)
	{
		FieldAttributes fieldAttributes = visibility.ToFieldAttributes() | (isStatic ? FieldAttributes.Static : default);
		FieldDefinition field = new FieldDefinition(fieldName, fieldAttributes, fieldType);
		type.Fields.Add(field);
		return field;
	}

	public static MethodDefinition AddMethod(this TypeDefinition type, string methodName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		MethodDefinition method = CreateMethod(methodName, methodAttributes, returnType);
		type.Methods.Add(method);
		return method;
	}

	public static MethodDefinition AddConstructor(this TypeDefinition type, Visibility visibility = Visibility.Public)
	{
		MethodAttributes methodAttributes = visibility.ToMethodAttributes() | MethodAttributes.SpecialName | MethodAttributes.RuntimeSpecialName;
		return type.AddMethod(".ctor", methodAttributes, type.Module!.CorLibTypeFactory.Void);
	}

	private static MethodDefinition CreateMethod(string methodName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		bool isStatic = (methodAttributes & MethodAttributes.Static) != 0;
		MethodSignature methodSignature = isStatic
			? MethodSignature.CreateStatic(returnType)
			: MethodSignature.CreateInstance(returnType);
		MethodDefinition result = new MethodDefinition(methodName, methodAttributes, methodSignature);

		result.CilMethodBody = new CilMethodBody(result);

		return result;
	}

	public static MethodDefinition GetDefaultConstructor(this TypeDefinition type)
	{
		return type.Methods.First(m => m.IsInstanceConstructor() && m.Parameters.Count == 0);
	}

	public static bool IsStaticClass(this TypeDefinition type)
	{
		return type.IsSealed && type.IsAbstract;
	}
}
