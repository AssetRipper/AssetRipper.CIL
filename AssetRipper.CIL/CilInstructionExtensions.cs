using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL;

public static class CilInstructionExtensions
{
	public static bool IsLdcI4OrI8(this CilInstruction instruction, out long value)
	{
		if (instruction.IsLdcI4())
		{
			value = instruction.GetLdcI4Constant();
			return true;
		}
		else if (instruction.OpCode == CilOpCodes.Ldc_I8 && instruction.Operand is long i64)
		{
			value = i64;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}

	public static bool IsLdcI4(this CilInstruction instruction, out int value)
	{
		if (instruction.IsLdcI4())
		{
			value = instruction.GetLdcI4Constant();
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}

	public static bool IsLdcI8(this CilInstruction instruction, out long value)
	{
		if (instruction.OpCode == CilOpCodes.Ldc_I8 && instruction.Operand is long i64)
		{
			value = i64;
			return true;
		}
		else
		{
			value = default;
			return false;
		}
	}
}
