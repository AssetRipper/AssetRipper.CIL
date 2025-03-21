using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL;

public enum Visibility
{
	Public,
	Protected,
	Internal,
	ProtectedInternal,
	Private,
	PrivateProtected
}
public static class VisibilityExtensions
{
	public static MethodAttributes ToMethodAttributes(this Visibility visibility) => visibility switch
	{
		Visibility.Public => MethodAttributes.Public,
		Visibility.Protected => MethodAttributes.Family,
		Visibility.Internal => MethodAttributes.Assembly,
		Visibility.ProtectedInternal => MethodAttributes.FamilyOrAssembly,
		Visibility.Private => MethodAttributes.Private,
		Visibility.PrivateProtected => MethodAttributes.FamilyAndAssembly,
		_ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null),
	};

	public static FieldAttributes ToFieldAttributes(this Visibility visibility) => visibility switch
	{
		Visibility.Public => FieldAttributes.Public,
		Visibility.Protected => FieldAttributes.Family,
		Visibility.Internal => FieldAttributes.Assembly,
		Visibility.ProtectedInternal => FieldAttributes.FamilyOrAssembly,
		Visibility.Private => FieldAttributes.Private,
		Visibility.PrivateProtected => FieldAttributes.FamilyAndAssembly,
		_ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null),
	};

	public static TypeAttributes ToTypeAttributes(this Visibility visibility, bool nested = false) => nested
		? visibility switch
		{
			Visibility.Public => TypeAttributes.NestedPublic,
			Visibility.Protected => TypeAttributes.NestedFamily,
			Visibility.Internal => TypeAttributes.NestedAssembly,
			Visibility.ProtectedInternal => TypeAttributes.NestedFamilyOrAssembly,
			Visibility.Private => TypeAttributes.NestedPrivate,
			Visibility.PrivateProtected => TypeAttributes.NestedFamilyAndAssembly,
			_ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null),
		}
		: visibility switch
		{
			Visibility.Public => TypeAttributes.Public,
			Visibility.Internal => TypeAttributes.NotPublic,
			_ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null),
		};
}
