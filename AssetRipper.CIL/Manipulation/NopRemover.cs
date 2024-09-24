using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Manipulation;

/// <summary>
/// This removes all nop instructions.
/// </summary>
/// <remarks>
/// This is a stronger version of <see cref="UnreferencedNopRemover"/>.
/// </remarks>
public sealed class NopRemover : ICilManipulator
{
	public void Execute(CilManipulationContext context)
	{
		int i = 1;
		while (i < context.Instructions.Count)
		{
			CilInstruction previousInstruction = context.Instructions[i - 1];
			if (previousInstruction.OpCode == CilOpCodes.Nop)
			{
				CilInstruction currentInstruction = context.Instructions[i];
				context.ReassignReferences(previousInstruction, currentInstruction);
				context.Remove(previousInstruction);
			}
			else
			{
				i++;
			}
		}
	}
}
