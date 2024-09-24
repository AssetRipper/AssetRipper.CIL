using AsmResolver.DotNet;

namespace AssetRipper.CIL.Standardizer
{
	internal class Program
	{
		static void Main(string[] args)
		{
			ModuleDefinition module = ModuleDefinition.FromFile(args[0]);
			foreach (MethodDefinition method in module.GetAllTypes().SelectMany(t => t.Methods))
			{
				method.CilMethodBody?.Instructions.StandardizeMacros();
			}
			module.Write(args[1]);
			Console.WriteLine("Done!");
		}
	}
}
