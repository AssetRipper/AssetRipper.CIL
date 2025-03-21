using AsmResolver.DotNet.Signatures;

namespace AssetRipper.CIL;

internal static class CorLibTypeFactoryExtensions
{
	public static CorLibTypeSignature? FromType(this CorLibTypeFactory factory, Type type)
	{
		if (type == typeof(bool))
		{
			return factory.Boolean;
		}
		if (type == typeof(char))
		{
			return factory.Char;
		}
		if (type == typeof(byte))
		{
			return factory.Byte;
		}
		if (type == typeof(sbyte))
		{
			return factory.SByte;
		}
		if (type == typeof(short))
		{
			return factory.Int16;
		}
		if (type == typeof(ushort))
		{
			return factory.UInt16;
		}
		if (type == typeof(int))
		{
			return factory.Int32;
		}
		if (type == typeof(uint))
		{
			return factory.UInt32;
		}
		if (type == typeof(long))
		{
			return factory.Int64;
		}
		if (type == typeof(ulong))
		{
			return factory.UInt64;
		}
		if (type == typeof(float))
		{
			return factory.Single;
		}
		if (type == typeof(double))
		{
			return factory.Double;
		}
		if (type == typeof(string))
		{
			return factory.String;
		}
		if (type == typeof(IntPtr))
		{
			return factory.IntPtr;
		}
		if (type == typeof(UIntPtr))
		{
			return factory.UIntPtr;
		}
		if (type == typeof(object))
		{
			return factory.Object;
		}
		if (type == typeof(void))
		{
			return factory.Void;
		}
		if (type == typeof(TypedReference))
		{
			return factory.TypedReference;
		}
		return null;
	}
}
