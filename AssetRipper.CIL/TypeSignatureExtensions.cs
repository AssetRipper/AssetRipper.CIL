using AsmResolver.DotNet.Signatures;

namespace AssetRipper.CIL;

internal static class TypeSignatureExtensions
{
	public static bool IsValueTypeOrGenericParameter(this TypeSignature type)
	{
		return type is { IsValueType: true } or GenericParameterSignature;
	}
}
