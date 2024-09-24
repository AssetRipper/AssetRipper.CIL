namespace AssetRipper.CIL.Manipulation;

public sealed class MacroOptimizer : ICilManipulator
{
	public void Execute(CilManipulationContext context) => context.Instructions.OptimizeMacros();
}
