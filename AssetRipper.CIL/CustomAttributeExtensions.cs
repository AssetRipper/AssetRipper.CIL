using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AssetRipper.CIL;

public static class CustomAttributeExtensions
{
	public static CustomAttributeArgument AddFixedArgument(this CustomAttribute attribute, TypeSignature paramType, object paramValue)
	{
		attribute.Signature ??= new();
		CustomAttributeArgument argument = new(paramType, paramValue);
		attribute.Signature.FixedArguments.Add(argument);
		return argument;
	}
}
