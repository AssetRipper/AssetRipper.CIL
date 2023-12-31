using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures.Types;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.CIL;

internal static class ParameterExtensions
{
	/// <summary>
	/// Is this <see cref="Parameter"/> an out parameter?
	/// </summary>
	/// <param name="parameter"></param>
	/// <param name="parameterType">The base type of the <see cref="ByReferenceTypeSignature"/></param>
	/// <returns></returns>
	public static bool IsOutParameter(this Parameter parameter, [NotNullWhen(true)] out TypeSignature? parameterType)
	{
		if ((parameter.Definition?.IsOut ?? false) && parameter.ParameterType is ByReferenceTypeSignature byReferenceTypeSignature)
		{
			parameterType = byReferenceTypeSignature.BaseType;
			return true;
		}
		else
		{
			parameterType = default;
			return false;
		}
	}
}
