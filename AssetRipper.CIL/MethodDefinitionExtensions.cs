using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

public static class MethodDefinitionExtensions
{
	/// <summary>
	/// Replaces the method body with a sensible minimal implementation.
	/// If the method is for an auto-property, the method will be replaced with a simple getter or setter.
	/// Otherwise, the method content will be filled with a valid default implementation that does not throw
	/// and handles all required logic, such as setting out parameters.
	/// </summary>
	/// <remarks>
	/// If the method is not managed or should not have a body, this method does nothing.
	/// </remarks>
	/// <param name="methodDefinition">The method to modify.</param>
	public static void ReplaceMethodBodyWithMinimalImplementation(this MethodDefinition methodDefinition)
	{
		MethodStubber.FillMethodBodyWithStub(methodDefinition);
	}

	/// <summary>
	/// Replaces the method body with: <code>throw null;</code>
	/// </summary>
	/// <remarks>
	/// If the method is not managed or should not have a body, this method does nothing.
	/// </remarks>
	/// <param name="methodDefinition">The method to modify.</param>
	public static void ReplaceMethodBodyWithThrowNull(this MethodDefinition methodDefinition)
	{
		if (!methodDefinition.IsManagedMethodWithBody())
		{
			return;
		}
		methodDefinition.CilMethodBody = new();
		CilInstructionCollection methodInstructions = methodDefinition.CilMethodBody.Instructions;
		methodInstructions.AddThrowNull();
	}

	public static bool IsInstance(this MethodDefinition methodDefinition)
	{
		return !methodDefinition.IsStatic;
	}

	public static bool IsInstanceConstructor(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsConstructor && methodDefinition.IsInstance();
	}

	public static bool IsStaticConstructor(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsConstructor && methodDefinition.IsStatic;
	}

	public static bool HasReturnType(this MethodDefinition methodDefinition)
	{
		return methodDefinition.Signature is not null
			&& methodDefinition.Signature.ReturnType is not CorLibTypeSignature { ElementType: ElementType.Void };
	}

	internal static bool ShouldCallBaseConstructor(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsInstanceConstructor() && !methodDefinition.IsDeclaredByValueType();
	}

	/// <summary>
	/// Is this method declared inside a struct?
	/// </summary>
	public static bool IsDeclaredByValueType(this MethodDefinition methodDefinition)
	{
		return methodDefinition.DeclaringType?.IsValueType is true;
	}

	internal static bool ShouldInitializeFields(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsInstanceConstructor() && methodDefinition.IsDeclaredByValueType();
	}

	public static bool IsManagedMethodWithBody(this MethodDefinition managedMethod)
	{
		return managedMethod.Managed
			&& !managedMethod.IsAbstract
			&& !managedMethod.IsPInvokeImpl
			&& !managedMethod.IsInternalCall
			&& !managedMethod.IsNative
			&& !managedMethod.IsRuntime;
	}

	public static CilInstructionCollection GetInstructions(this MethodDefinition method)
	{
		return method.CilMethodBody?.Instructions ?? throw new ArgumentException("CilMethodBody was null", nameof(method));
	}

	public static bool IsAutoPropertyGetMethod(this MethodDefinition method, [NotNullWhen(true)] out FieldDefinition? backingField)
	{
		if (!method.IsGetMethod
			|| method.DeclaringType is null
			|| method.Parameters.Count is not 0//Must have no parameters
			|| method.Name is null
			|| !method.Name.Value.StartsWith("get_", StringComparison.Ordinal))//Must adhere to the Roslyn naming convention
		{
			backingField = null;
			return false;
		}

		string backingFieldName = $"<{method.Name.Value[4..]}>k__BackingField";
		FieldDefinition? field = method.DeclaringType.Fields.FirstOrDefault(f => f.Name == backingFieldName);
		if (field is null
			|| !field.IsPrivate
			|| field.IsStatic != method.IsStatic
			|| method.DeclaringModule?.RuntimeContext is not { } runtimeContext
			|| !runtimeContext.SignatureComparer.Equals(field.Signature?.FieldType, method.Signature?.ReturnType))
		{
			backingField = null;
			return false;
		}
		else
		{
			backingField = field;
			return true;
		}
	}

	public static bool IsAutoPropertySetMethod(this MethodDefinition method, [NotNullWhen(true)] out FieldDefinition? backingField)
	{
		if (!method.IsSetMethod
			|| method.DeclaringType is null
			|| method.Signature?.ReturnType is not CorLibTypeSignature { ElementType: ElementType.Void }//Must return void
			|| method.Parameters.Count is not 1//Must have exactly one parameter
			|| method.Name is null
			|| !method.Name.Value.StartsWith("set_", StringComparison.Ordinal))//Must adhere to the Roslyn naming convention
		{
			backingField = null;
			return false;
		}

		string backingFieldName = $"<{method.Name.Value[4..]}>k__BackingField";
		FieldDefinition? field = method.DeclaringType.Fields.FirstOrDefault(f => f.Name == backingFieldName);
		if (field is null
			|| !field.IsPrivate
			|| field.IsStatic != method.IsStatic
			|| method.DeclaringModule?.RuntimeContext is not { } runtimeContext
			|| !runtimeContext.SignatureComparer.Equals(field.Signature?.FieldType, method.Parameters[0].ParameterType))
		{
			backingField = null;
			return false;
		}
		else
		{
			backingField = field;
			return true;
		}
	}

	public static Parameter AddParameter(this MethodDefinition method, TypeSignature parameterSignature, string? parameterName)
	{
		return method.AddParameter(parameterSignature, parameterName, out _);
	}

	public static Parameter AddParameter(this MethodDefinition method, TypeSignature parameterSignature, string? parameterName, out ParameterDefinition parameterDefinition)
	{
		parameterDefinition = new ParameterDefinition((ushort)(method.Signature!.ParameterTypes.Count + 1), parameterName, default);
		method.Signature.ParameterTypes.Add(parameterSignature);
		method.ParameterDefinitions.Add(parameterDefinition);

		method.Parameters.PullUpdatesFromMethodSignature();
		return method.Parameters.Single(parameter => parameter.Name == parameterName && parameter.ParameterType == parameterSignature);
	}

	public static Parameter AddParameter(this MethodDefinition method, TypeSignature parameterSignature)
	{
		method.Signature!.ParameterTypes.Add(parameterSignature);
		method.Parameters.PullUpdatesFromMethodSignature();
		return method.Parameters[^1];
	}
}
