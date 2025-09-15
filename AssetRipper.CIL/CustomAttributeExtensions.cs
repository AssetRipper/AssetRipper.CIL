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

	public static CustomAttributeArgument AddFixedArgument<T>(this CustomAttribute attribute, TypeSignature paramType, T[] paramValue)
	{
		CustomAttributeArgument argument = new CustomAttributeArgument(paramType, paramValue.Select(e => (object?)e));
		attribute.Signature!.FixedArguments.Add(argument);
		return argument;
	}

	public static CustomAttributeNamedArgument AddNamedArgument(this CustomAttribute attribute, TypeSignature memberType, string memberName, TypeSignature paramType, object paramValue, CustomAttributeArgumentMemberType memberKind)
	{
		CustomAttributeArgument argument = new CustomAttributeArgument(paramType, paramValue);
		CustomAttributeNamedArgument namedArgument = new CustomAttributeNamedArgument(memberKind, memberName, memberType, argument);
		attribute.Signature!.NamedArguments.Add(namedArgument);
		return namedArgument;
	}
}
