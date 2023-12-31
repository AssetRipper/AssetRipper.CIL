using AsmResolver.DotNet.Signatures.Types;

namespace AssetRipper.CIL;

internal static class TypeSignatureExtensions
{
	public static bool IsValueTypeOrGenericParameter(this TypeSignature type)
	{
		return type is { IsValueType: true } or GenericParameterSignature;
	}
}
