using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Manipulation;

/// <summary>
/// This manipulator removes switch instructions by converting them to a series of conditional branches.
/// </summary>
public sealed class SwitchRemover : ICilManipulator
{
	public void Execute(CilManipulationContext context)
	{
		ModuleDefinition? module = context.Instructions.Owner.Owner.Module;
		if (module is null)
		{
			return;
		}

		for (int i = 0; i < context.Instructions.Count; i++)
		{
			CilInstruction instruction = context.Instructions[i];
			if (instruction.OpCode == CilOpCodes.Switch && instruction.Operand is IReadOnlyList<ICilLabel> labels)
			{
				CilLocalVariable switchValue = context.Instructions.AddLocalVariable(module.CorLibTypeFactory.UInt32);

				context.Replace(instruction, new CilInstruction(CilOpCodes.Conv_U4));

				context.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Stloc, switchValue));
				i++; // No need to process the newly added instruction

				for (int j = 0; j < labels.Count; j++)
				{
					ICilLabel label = labels[j];
					context.Instructions.Insert(i + 1, new CilInstruction(CilOpCodes.Ldloc, switchValue));
					context.Instructions.Insert(i + 2, new CilInstruction(CilOpCodes.Ldc_I4, j));
					context.Instructions.Insert(i + 3, new CilInstruction(CilOpCodes.Ceq));
					context.Instructions.Insert(i + 4, new CilInstruction(CilOpCodes.Brtrue, label));
					i += 4; // No need to process the newly added instructions
				}
			}
		}
	}
}
