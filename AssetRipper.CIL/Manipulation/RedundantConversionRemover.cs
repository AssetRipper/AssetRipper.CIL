using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Manipulation;

public sealed class RedundantConversionRemover : ICilManipulator
{
	public void Execute(CilManipulationContext context)
	{
		int i = 1;
		while (i < context.Instructions.Count)
		{
			CilInstruction previousInstruction = context.Instructions[i - 1];
			CilInstruction instruction = context.Instructions[i];
			if (previousInstruction.OpCode == instruction.OpCode
				&& instruction.OpCode.Code
					is CilCode.Conv_I1 or CilCode.Conv_I2 or CilCode.Conv_I4 or CilCode.Conv_I8
					or CilCode.Conv_U1 or CilCode.Conv_U2 or CilCode.Conv_U4 or CilCode.Conv_U8
					or CilCode.Conv_R4 or CilCode.Conv_R8
					or CilCode.Conv_I or CilCode.Conv_U
				&& (context.TryRemove(instruction) || context.TryRemove(previousInstruction)))
			{
			}
			else
			{
				i++;
			}
		}
	}
}
