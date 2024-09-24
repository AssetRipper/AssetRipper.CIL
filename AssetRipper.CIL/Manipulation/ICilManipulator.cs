namespace AssetRipper.CIL.Manipulation;

/// <summary>
/// An interface for manipulating CIL instructions.
/// </summary>
/// <remarks>
/// Derived classes are expected to be stateless and have a parameterless constructor.
/// </remarks>
public interface ICilManipulator
{
	void Execute(CilManipulationContext context);
}
