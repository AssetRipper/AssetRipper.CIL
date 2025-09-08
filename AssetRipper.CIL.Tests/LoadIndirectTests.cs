using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL.Tests;

public class LoadIndirectTests
{
	[TestCase(ElementType.I1, CilCode.Ldind_I1)]
	[TestCase(ElementType.I2, CilCode.Ldind_I2)]
	[TestCase(ElementType.I4, CilCode.Ldind_I4)]
	[TestCase(ElementType.I8, CilCode.Ldind_I8)]
	[TestCase(ElementType.U1, CilCode.Ldind_U1)]
	[TestCase(ElementType.U2, CilCode.Ldind_U2)]
	[TestCase(ElementType.U4, CilCode.Ldind_U4)]
	[TestCase(ElementType.U8, CilCode.Ldind_I8)]
	[TestCase(ElementType.I, CilCode.Ldind_I)]
	[TestCase(ElementType.U, CilCode.Ldind_I)]
	[TestCase(ElementType.R4, CilCode.Ldind_R4)]
	[TestCase(ElementType.R8, CilCode.Ldind_R8)]
	[TestCase(ElementType.Boolean, CilCode.Ldind_U1)]
	[TestCase(ElementType.Char, CilCode.Ldind_U2)]
	[TestCase(ElementType.Object, CilCode.Ldind_Ref)]
	[TestCase(ElementType.String, CilCode.Ldind_Ref)]
	[TestCase(ElementType.TypedByRef, CilCode.Ldobj)]
	public void CorlibLoadIndirect(ElementType type, CilCode code)
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		CorLibTypeSignature? typeToLoad = module.CorLibTypeFactory.FromElementType(type);
		Assert.That(typeToLoad, Is.Not.Null);
		AssertCorrectLoadIndirect(typeToLoad, code);
	}

	[TestCase(ElementType.I1, CilCode.Ldind_I1)]
	[TestCase(ElementType.I2, CilCode.Ldind_I2)]
	[TestCase(ElementType.I4, CilCode.Ldind_I4)]
	[TestCase(ElementType.I8, CilCode.Ldind_I8)]
	[TestCase(ElementType.U1, CilCode.Ldind_U1)]
	[TestCase(ElementType.U2, CilCode.Ldind_U2)]
	[TestCase(ElementType.U4, CilCode.Ldind_U4)]
	[TestCase(ElementType.U8, CilCode.Ldind_I8)]
	public void EnumLoadIndirect(ElementType type, CilCode code)
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeSignature? typeToLoad = CreateEnum(module, type).ToTypeSignature();
		Assert.That(typeToLoad, Is.Not.Null);
		AssertCorrectLoadIndirect(typeToLoad, code);
	}

	[Test]
	public void PointerLoadIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		PointerTypeSignature typeToLoad = module.CorLibTypeFactory.Void.MakePointerType();
		AssertCorrectLoadIndirect(typeToLoad, CilCode.Ldind_I);
	}

	[Test]
	public void GenericParameterLoadIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		GenericParameterSignature typeToLoad = new GenericParameterSignature(module, GenericParameterType.Type, 0);
		AssertCorrectLoadIndirect(typeToLoad, CilCode.Ldobj);
	}

	[Test]
	public void SzArrayLoadIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeSignature typeToLoad = module.CorLibTypeFactory.Int32.MakeSzArrayType();
		AssertCorrectLoadIndirect(typeToLoad, CilCode.Ldind_Ref);
	}

	[Test]
	public void ClassLoadIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeDefinition type = new TypeDefinition("TestNamespace", "TestName", TypeAttributes.Public, module.CorLibTypeFactory.Object.ToTypeDefOrRef());
		module.TopLevelTypes.Add(type);
		AssertCorrectLoadIndirect(type.ToTypeSignature(), CilCode.Ldind_Ref);
	}

	[Test]
	public void StructLoadIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeDefinition type = new TypeDefinition("TestNamespace", "TestName", TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed, module.DefaultImporter.ImportType(typeof(ValueType)));
		module.TopLevelTypes.Add(type);
		AssertCorrectLoadIndirect(type.ToTypeSignature(), CilCode.Ldobj);
	}

	private static void AssertCorrectLoadIndirect(TypeSignature typeToLoad, CilCode expectedOpCode)
	{
		MethodDefinition method = new MethodDefinition("Test", MethodAttributes.Public | MethodAttributes.Static, null);
		method.CilMethodBody = new();
		CilInstructionCollection instructions = method.CilMethodBody.Instructions;
		instructions.AddLoadIndirect(typeToLoad);
		Assert.That(instructions, Has.Count.EqualTo(1));
		Assert.That(instructions[0].OpCode.Code, Is.EqualTo(expectedOpCode));
	}

	private static TypeDefinition CreateEnum(ModuleDefinition module, ElementType underlyingType)
	{
		TypeDefinition enumType = new TypeDefinition("TestNamespace", "TestEnum", TypeAttributes.Public, module.DefaultImporter.ImportType(typeof(Enum)));
		module.TopLevelTypes.Add(enumType);
		CorLibTypeSignature underlyingTypeSignature = module.CorLibTypeFactory.FromElementType(underlyingType) ?? throw new NullReferenceException();
		FieldDefinition valueField = new FieldDefinition("value__", FieldAttributes.Public | FieldAttributes.SpecialName | FieldAttributes.RuntimeSpecialName, underlyingTypeSignature);
		enumType.Fields.Add(valueField);
		return enumType;
	}
}
