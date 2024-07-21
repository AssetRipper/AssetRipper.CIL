using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL;

public static class MacroStandardization
{
	/// <summary>
	/// This method expands all macros in the collection and applies some standardizations, such as removing nop instructions.
	/// </summary>
	/// <param name="instructions">The collection to modify.</param>
	public static void StandardizeMacros(this CilInstructionCollection instructions)
	{
		instructions.ExpandMacros();
		instructions.RemoveNopInstructions();
	}

	private static void RemoveNopInstructions(this CilInstructionCollection instructions)
	{
		// Find labels that reference nop instructions.
		HashSet<CilInstructionLabel> nopLabels = new();
		foreach (CilInstruction instruction in instructions)
		{
			if (instruction.Operand is CilInstructionLabel { Instruction: not null } label && label.Instruction.OpCode == CilOpCodes.Nop)
			{
				nopLabels.Add(label);
			}
			else if (instruction.Operand is CilInstructionLabel[] labels) // switch
			{
				foreach (CilInstructionLabel l in labels)
				{
					if (l.Instruction is not null && l.Instruction.OpCode == CilOpCodes.Nop)
					{
						nopLabels.Add(l);
					}
				}
			}
		}

		// Reassign labels to the next instruction, which is not a nop.
		foreach (CilInstructionLabel label in nopLabels)
		{
			int index = instructions.IndexOf(label.Instruction!);
			if (index < 0)
			{
				throw new InvalidOperationException("Label instruction not found");
			}

			for (int i = index + 1; i < instructions.Count; i++)
			{
				if (instructions[i].OpCode != CilOpCodes.Nop)
				{
					label.Instruction = instructions[i];
					break;
				}
			}
		}

		// Remove nop instructions
		for (int i = instructions.Count - 1; i >= 0; i--)
		{
			if (instructions[i].OpCode == CilOpCodes.Nop)
			{
				instructions.RemoveAt(i);
			}
		}
	}
}
