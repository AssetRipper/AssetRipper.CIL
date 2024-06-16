using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

internal static class MethodStubber
{
	public static void FillMethodBodyWithStub(MethodDefinition methodDefinition)
	{
		if (!methodDefinition.IsManagedMethodWithBody())
		{
			return;
		}

		methodDefinition.CilMethodBody = new(methodDefinition);
		CilInstructionCollection methodInstructions = methodDefinition.CilMethodBody.Instructions;

		if (methodDefinition.IsAutoPropertyGetMethod(out FieldDefinition? backingField))
		{
			if (methodDefinition.IsStatic)
			{
				methodInstructions.Add(CilOpCodes.Ldsfld, backingField);
			}
			else
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				methodInstructions.Add(CilOpCodes.Ldfld, backingField);
			}
			methodInstructions.Add(CilOpCodes.Ret);
			return;
		}
		else if (methodDefinition.IsAutoPropertySetMethod(out backingField))
		{
			if (methodDefinition.IsStatic)
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				methodInstructions.Add(CilOpCodes.Stsfld, backingField);
			}
			else
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				methodInstructions.Add(CilOpCodes.Ldarg_1);
				methodInstructions.Add(CilOpCodes.Stfld, backingField);
			}
			methodInstructions.Add(CilOpCodes.Ret);
			return;
		}

		if (methodDefinition.ShouldInitializeFields())
		{
			TypeDefinition declaringType = methodDefinition.DeclaringType!;
			foreach (FieldDefinition field in declaringType.Fields)
			{
				if (field.IsStatic || field.Signature is null)
				{
					continue;
				}

				methodInstructions.Add(CilOpCodes.Ldarg_0);
				methodInstructions.AddDefaultValue(field.Signature.FieldType);
				methodInstructions.Add(CilOpCodes.Stfld, field);
			}
		}

		if (methodDefinition.ShouldCallBaseConstructor() && TryGetBaseConstructor(methodDefinition, out IMethodDescriptor? baseConstructor))
		{
			MethodSignature? methodSignature = baseConstructor.Signature;
			if (methodSignature is not null)
			{
				methodInstructions.Add(CilOpCodes.Ldarg_0);
				foreach (TypeSignature baseParameterType in methodSignature.ParameterTypes)
				{
					methodInstructions.AddDefaultValue(baseParameterType);
				}
				methodInstructions.Add(CilOpCodes.Call, baseConstructor);
			}
		}

		foreach (Parameter parameter in methodDefinition.Parameters)
		{
			//Although Roslyn-compiled code will only emit the out flag on ByReferenceTypeSignatures,
			//Some Unity libraries have it on a handful (less than 100) of parameters with incompatible type signatures.
			//One example on 2021.3.6 is int System.IO.CStreamReader.Read([In][Out] char[] dest, int index, int count)
			//All the instances I investigated were clearly not meant to be out parameters.
			//The [In][Out] attributes are not a decompilation issue and compile fine on .NET 7.
			if (parameter.IsOutParameter(out TypeSignature? parameterType))
			{
				if (parameterType.IsValueTypeOrGenericParameter())
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Initobj, parameterType.ToTypeDefOrRef());
				}
				else
				{
					methodInstructions.Add(CilOpCodes.Ldarg, parameter);
					methodInstructions.Add(CilOpCodes.Ldnull);
					methodInstructions.Add(CilOpCodes.Stind_Ref);
				}
			}
		}
		methodInstructions.AddDefaultValue(methodDefinition.Signature!.ReturnType);
		methodInstructions.Add(CilOpCodes.Ret);
		methodInstructions.OptimizeMacros();
	}

	private static IMethodDescriptor? TryGetBaseConstructor(MethodDefinition methodDefinition)
	{
		if (methodDefinition.DeclaringType is null)
		{
			return null;
		}
		ITypeDefOrRef? baseType = methodDefinition.DeclaringType.BaseType;
		if (baseType is null)
		{
			return null;
		}

		if (baseType is TypeDefinition baseTypeDef)
		{
			return GetFirstCompatibleConstructor(methodDefinition.DeclaringType, baseTypeDef);
		}
		else if (baseType is TypeReference baseTypeRef)
		{
			TypeDefinition? baseTypeResolved = baseTypeRef.Resolve();
			if (baseTypeResolved is null)
			{
				return null;
			}
			MethodDefinition? baseConstructor = GetFirstCompatibleConstructor(methodDefinition.DeclaringType, baseTypeResolved);
			if (baseConstructor is null || methodDefinition.DeclaringType.Module is null)
			{
				return null;
			}
			else
			{
				return methodDefinition.DeclaringType.Module.DefaultImporter.ImportMethod(baseConstructor);
			}
		}
		else if (baseType is TypeSpecification baseTypeSpec)
		{
			TypeSignature? baseTypeSignature = baseTypeSpec.Signature;
			if (baseTypeSignature is not GenericInstanceTypeSignature genericInstanceTypeSignature)
			{
				return null;
			}
			TypeDefinition? baseTypeResolved = genericInstanceTypeSignature.GenericType.Resolve();
			if (baseTypeResolved is null)
			{
				return null;
			}
			MethodDefinition? baseConstructor = GetFirstCompatibleConstructor(methodDefinition.DeclaringType, baseTypeResolved);
			if (baseConstructor is null || methodDefinition.DeclaringType.Module is null)
			{
				return null;
			}
			IMethodDefOrRef baseConstructorImported = methodDefinition.DeclaringType.Module.DefaultImporter.ImportMethod(baseConstructor);
			return new MemberReference(baseType, baseConstructorImported.Name, baseConstructorImported.Signature);
		}

		return null;

		static MethodDefinition? GetFirstCompatibleConstructor(TypeDefinition declaringType, TypeDefinition baseType)
		{
			if (declaringType.Module == baseType.Module)
			{
				return baseType.Methods.FirstOrDefault(m => m.IsInstanceConstructor() && !m.IsPrivate);
			}
			else
			{
				return baseType.Methods.FirstOrDefault(m => m.IsInstanceConstructor() && (m.IsFamily || m.IsPublic));
			}
		}
	}

	private static bool TryGetBaseConstructor(MethodDefinition methodDefinition, [NotNullWhen(true)] out IMethodDescriptor? baseConstructor)
	{
		baseConstructor = TryGetBaseConstructor(methodDefinition);
		return baseConstructor is not null;
	}
}
