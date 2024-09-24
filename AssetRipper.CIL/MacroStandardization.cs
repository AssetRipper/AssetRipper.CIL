using AsmResolver.DotNet.Code.Cil;
using AssetRipper.CIL.Manipulation;

namespace AssetRipper.CIL;

public static class MacroStandardization
{
	/// <summary>
	/// This method expands all macros in the collection and applies some standardizations, such as removing nop and switch instructions.
	/// </summary>
	/// <param name="instructions">The collection to modify.</param>
	public static void StandardizeMacros(this CilInstructionCollection instructions)
	{
		new CilManipulationContext(instructions)
			.Execute<MacroExpander>()
			.Execute<NopRemover>()
			.Execute<SwitchRemover>()
			.Execute<RedundantConversionRemover>();
	}
}
