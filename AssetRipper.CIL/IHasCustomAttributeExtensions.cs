using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using System.Runtime.CompilerServices;

namespace AssetRipper.CIL;

public static class IHasCustomAttributeExtensions
{
	public static CustomAttribute AddCustomAttribute(this IHasCustomAttribute _this, IMethodDefOrRef constructor)
	{
		if (constructor is ICustomAttributeType usableConstructor)
		{
			CustomAttribute attribute = new(usableConstructor);
			_this.CustomAttributes.Add(attribute);
			return attribute;
		}
		else
		{
			throw new ArgumentException("Constructor is not ICustomAttributeType", nameof(constructor));
		}
	}

	public static CustomAttribute AddCustomAttribute(this IHasCustomAttribute _this, IMethodDefOrRef constructor, TypeSignature paramType, object paramValue)
	{
		CustomAttribute attribute = _this.AddCustomAttribute(constructor);
		attribute.AddFixedArgument(paramType, paramValue);
		return attribute;
	}

	public static CustomAttribute AddCustomAttribute<T>(this IHasCustomAttribute _this, IMethodDefOrRef constructor, SzArrayTypeSignature paramType, T[] paramValue)
	{
		CustomAttribute attribute = _this.AddCustomAttribute(constructor);
		attribute.AddFixedArgument(paramType, paramValue);
		return attribute;
	}

	public static CustomAttribute AddCustomAttribute(this IHasCustomAttribute _this, IMethodDefOrRef constructor, params (TypeSignature, object)[] parameters)
	{
		CustomAttribute attribute = _this.AddCustomAttribute(constructor);
		for (int i = 0; i < parameters.Length; i++)
		{
			attribute.AddFixedArgument(parameters[i].Item1, parameters[i].Item2);
		}
		return attribute;
	}

	internal static ModuleDefinition GetModule(this IHasCustomAttribute _this)
	{
		return (_this as IModuleProvider)?.ContextModule ?? throw new ArgumentException("Entity does not have a module", nameof(_this));
	}

	public static CustomAttribute AddFlagsAttribute(this IHasCustomAttribute _this)
	{
		ModuleDefinition module = _this.GetModule();

		if (module.TryGetTopLevelType("System", nameof(FlagsAttribute), out TypeDefinition? flagsType))
		{
			return _this.AddCustomAttribute(flagsType.GetDefaultConstructor());
		}
		else
		{
			TypeReference flagsTypeRef = new(module.CorLibTypeFactory.CorLibScope, "System", nameof(FlagsAttribute));
			MemberReference constructor = new(flagsTypeRef, ".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
			return _this.AddCustomAttribute(module.DefaultImporter.ImportMethod(constructor));
		}
	}

	/// <summary>
	/// Applies the <see cref="CompilerGeneratedAttribute"/>
	/// </summary>
	/// <param name="_this">The entity on which to apply the attribute</param>
	/// <returns>The resulting custom attribute</returns>
	public static CustomAttribute AddCompilerGeneratedAttribute(this IHasCustomAttribute _this)
	{
		ModuleDefinition module = _this.GetModule();

		if (module.TryGetTopLevelType("System.Runtime.CompilerServices", nameof(CompilerGeneratedAttribute), out TypeDefinition? compilerGeneratedType))
		{
			return _this.AddCustomAttribute(compilerGeneratedType.GetDefaultConstructor());
		}
		else
		{
			AssemblyReference compilerServicesAssembly = GetOrAddReferenceToSystemRuntimeCompilerServices(module);

			TypeReference compilerGeneratedTypeRef = new(compilerServicesAssembly, "System.Runtime.CompilerServices", nameof(CompilerGeneratedAttribute));

			MemberReference constructor = new(compilerGeneratedTypeRef, ".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));

			return _this.AddCustomAttribute(module.DefaultImporter.ImportMethod(constructor));
		}

		static AssemblyReference GetOrAddReferenceToSystemRuntimeCompilerServices(ModuleDefinition module)
		{
			AssemblyReference? compilerServicesAssembly = module.AssemblyReferences.FirstOrDefault(a => a.Name == "System.Runtime.CompilerServices");
			if (compilerServicesAssembly is null)
			{
				Version version = module.CorLibTypeFactory.CorLibScope.GetAssembly()?.Version ?? new Version(0, 0, 0, 0);
				compilerServicesAssembly = new("System.Runtime.CompilerServices", version);
				module.AssemblyReferences.Add(compilerServicesAssembly);
			}
			return compilerServicesAssembly;
		}
	}
}
