using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

internal static class MethodDefinitionExtensions
{
	public static bool IsInstanceConstructor(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsConstructor && !methodDefinition.IsStatic;
	}

	public static bool ShouldCallBaseConstructor(this MethodDefinition methodDefinition)
	{
		return methodDefinition.IsInstanceConstructor() && methodDefinition.DeclaringType?.IsValueType is not true;
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
		if (field is null || !SignatureComparer.Default.Equals(field.Signature?.FieldType, method.Signature?.ReturnType))
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
		if (field is null || !SignatureComparer.Default.Equals(field.Signature?.FieldType, method.Parameters[0].ParameterType))
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
}
