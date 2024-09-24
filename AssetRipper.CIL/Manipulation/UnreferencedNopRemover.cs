using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Manipulation;

/// <summary>
/// This removes unreferenced nop instructions.
/// </summary>
public sealed class UnreferencedNopRemover : ICilManipulator
{
	public void Execute(CilManipulationContext context)
	{
		int i = 0;
		while (i < context.Instructions.Count)
		{
			CilInstruction instruction = context.Instructions[i];
			if (instruction.OpCode == CilOpCodes.Nop && context.TryRemove(instruction))
			{
			}
			else
			{
				i++;
			}
		}
	}
}
