using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Manipulation;

public sealed class CilManipulationContext
{
	private readonly Dictionary<CilInstruction, HashSet<CilInstructionLabel>> instructionReferences = new();
	public CilInstructionCollection Instructions { get; }

	public CilManipulationContext(CilInstructionCollection instructions)
	{
		Instructions = instructions;

		foreach (CilExceptionHandler handler in instructions.Owner.ExceptionHandlers)
		{
			AddReference(handler.TryStart as CilInstructionLabel);
			AddReference(handler.TryEnd as CilInstructionLabel);
			AddReference(handler.HandlerStart as CilInstructionLabel);
			AddReference(handler.HandlerEnd as CilInstructionLabel);
			AddReference(handler.FilterStart as CilInstructionLabel);
		}

		foreach (CilInstruction instruction in instructions)
		{
			if (instruction.Operand is CilInstructionLabel label)
			{
				AddReference(label);
			}
			else if (instruction.Operand is IReadOnlyList<ICilLabel> labels) // switch
			{
				foreach (ICilLabel l in labels)
				{
					AddReference(l as CilInstructionLabel);
				}
			}
		}
	}

	public bool CanRemove(CilInstruction instruction) => !instructionReferences.ContainsKey(instruction);

	public void Remove(CilInstruction instruction)
	{
		if (!CanRemove(instruction))
		{
			throw new InvalidOperationException("Cannot remove instruction with references");
		}
		Instructions.Remove(instruction);
	}

	public bool TryRemove(CilInstruction instruction)
	{
		return CanRemove(instruction) && Instructions.Remove(instruction);
	}

	public void Replace(CilInstruction oldInstruction, CilInstruction newInstruction)
	{
		if (instructionReferences.TryGetValue(oldInstruction, out HashSet<CilInstructionLabel>? references))
		{
			instructionReferences.Remove(oldInstruction);
			instructionReferences.Add(newInstruction, references);
			foreach (CilInstructionLabel label in references)
			{
				label.Instruction = newInstruction;
			}
		}
		Instructions[Instructions.IndexOf(oldInstruction)] = newInstruction;
	}

	public void ReassignReferences(CilInstruction oldInstruction, CilInstruction newInstruction)
	{
		if (instructionReferences.TryGetValue(oldInstruction, out HashSet<CilInstructionLabel>? oldReferences))
		{
			instructionReferences.Remove(oldInstruction);
			foreach (CilInstructionLabel label in oldReferences)
			{
				label.Instruction = newInstruction;
			}

			if (instructionReferences.TryGetValue(newInstruction, out HashSet<CilInstructionLabel>? newReferences))
			{
				newReferences.UnionWith(oldReferences);
			}
			else
			{
				instructionReferences.Add(newInstruction, oldReferences);
			}
		}
	}

	public CilManipulationContext Execute<T>() where T : ICilManipulator, new()
	{
		SingletonCache<T>.Instance.Execute(this);
		return this;
	}

	private void AddReference(CilInstructionLabel? label)
	{
		CilInstruction? instruction = label?.Instruction;
		if (instruction is not null)
		{
			instructionReferences.GetOrCreate(instruction).Add(label!);
		}
	}
}
