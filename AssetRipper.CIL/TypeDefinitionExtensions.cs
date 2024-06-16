using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL;

public static class TypeDefinitionExtensions
{
	public static MethodDefinition AddMethod(this TypeDefinition type, string methodName, MethodAttributes methodAttributes, TypeSignature returnType)
	{
		MethodDefinition method = CreateMethod(methodName, methodAttributes, returnType);
		type.Methods.Add(method);
		return method;
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
}
