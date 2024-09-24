namespace AssetRipper.CIL.Manipulation;

public sealed class MacroExpander : ICilManipulator
{
	public void Execute(CilManipulationContext context) => context.Instructions.ExpandMacros();
}
