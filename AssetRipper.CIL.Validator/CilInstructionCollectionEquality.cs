using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;

namespace AssetRipper.CIL.Validator;

public static class CilInstructionCollectionEquality
{
	public static bool Equals(CilMethodBody left, CilMethodBody right)
	{
		if (left.LocalVariables.Count != right.LocalVariables.Count)
		{
			return false;
		}
		for (int i = 0; i < left.LocalVariables.Count; i++)
		{
			if (!SignatureComparer.Default.Equals(left.LocalVariables[i].VariableType, right.LocalVariables[i].VariableType))
			{
				return false;
			}
		}

		return Equals(left.Instructions, right.Instructions); 
	}

	public static bool Equals(CilInstructionCollection left, CilInstructionCollection right)
	{
		if (left.Count != right.Count)
		{
			return false;
		}

		for (int i = 0; i < left.Count; i++)
		{
			CilInstruction leftInstruction = left[i];
			CilInstruction rightInstruction = right[i];
			if (leftInstruction.OpCode != rightInstruction.OpCode || leftInstruction.Operand?.GetType() != rightInstruction.Operand?.GetType())
			{
				return false;
			}

			if (rightInstruction.Operand is null)
			{
				// Both operands are null because of the type check.
				continue;
			}

			switch (leftInstruction.Operand)
			{
				case CilInstructionLabel leftLabel:
					{
						CilInstructionLabel rightLabel = (CilInstructionLabel)rightInstruction.Operand;
						int leftIndex = leftLabel.Instruction is null ? -1 : left.IndexOf(leftLabel.Instruction);
						int rightIndex = rightLabel.Instruction is null ? -1 : right.IndexOf(rightLabel.Instruction);

						if (leftIndex != rightIndex)
						{
							return false;
						}
					}
					break;
				case CilInstructionLabel[] leftLabels:
					{
						CilInstructionLabel[] rightLabels = (CilInstructionLabel[])rightInstruction.Operand;
						if (leftLabels.Length != rightLabels.Length)
						{
							return false;
						}

						for (int j = 0; j < leftLabels.Length; j++)
						{
							CilInstructionLabel leftLabel = leftLabels[j];
							CilInstructionLabel rightLabel = rightLabels[j];
							int leftIndex = leftLabel.Instruction is null ? -1 : left.IndexOf(leftLabel.Instruction);
							int rightIndex = rightLabel.Instruction is null ? -1 : right.IndexOf(rightLabel.Instruction);

							if (leftIndex != rightIndex)
							{
								return false;
							}
						}
					}
					break;
				case Parameter leftParameter:
					{
						Parameter rightParameter = (Parameter)rightInstruction.Operand;
						if (leftParameter.Index != rightParameter.Index)
						{
							return false;
						}
					}
					break;
				case CilLocalVariable leftVariable:
					{
						CilLocalVariable rightVariable = (CilLocalVariable)rightInstruction.Operand;
						if (left.Owner.LocalVariables.IndexOf(leftVariable) != right.Owner.LocalVariables.IndexOf(rightVariable))
						{
							return false;
						}
					}
					break;
				case MemberReference leftMember:
					{
						MemberReference rightMember = (MemberReference)rightInstruction.Operand;
						if (!SignatureComparer.Default.Equals(leftMember, rightMember))
						{
							return false;
						}
					}
					break;
				case IFieldDescriptor leftField:
					{
						IFieldDescriptor rightField = (IFieldDescriptor)rightInstruction.Operand;
						if (!SignatureComparer.Default.Equals(leftField, rightField))
						{
							return false;
						}
					}
					break;
				case ITypeDefOrRef leftType:
					{
						ITypeDefOrRef rightType = (ITypeDefOrRef)rightInstruction.Operand;
						if (!SignatureComparer.Default.Equals(leftType, rightType))
						{
							return false;
						}
					}
					break;
				case IMethodDescriptor leftMethod:
					{
						IMethodDescriptor rightMethod = (IMethodDescriptor)rightInstruction.Operand;
						if (!SignatureComparer.Default.Equals(leftMethod, rightMethod))
						{
							return false;
						}
					}
					break;
				default:
					if (!leftInstruction.Operand!.Equals(rightInstruction.Operand))
					{
						return false;
					}
					break;
			}
		}

		return true;
	}
}
