using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.CIL.Tests;

public class StoreIndirectTests
{
	[TestCase(ElementType.I1, CilCode.Stind_I1)]
	[TestCase(ElementType.I2, CilCode.Stind_I2)]
	[TestCase(ElementType.I4, CilCode.Stind_I4)]
	[TestCase(ElementType.I8, CilCode.Stind_I8)]
	[TestCase(ElementType.U1, CilCode.Stind_I1)]
	[TestCase(ElementType.U2, CilCode.Stind_I2)]
	[TestCase(ElementType.U4, CilCode.Stind_I4)]
	[TestCase(ElementType.U8, CilCode.Stind_I8)]
	[TestCase(ElementType.I, CilCode.Stind_I)]
	[TestCase(ElementType.U, CilCode.Stind_I)]
	[TestCase(ElementType.R4, CilCode.Stind_R4)]
	[TestCase(ElementType.R8, CilCode.Stind_R8)]
	[TestCase(ElementType.Boolean, CilCode.Stind_I1)]
	[TestCase(ElementType.Char, CilCode.Stind_I2)]
	[TestCase(ElementType.Object, CilCode.Stind_Ref)]
	[TestCase(ElementType.String, CilCode.Stind_Ref)]
	[TestCase(ElementType.TypedByRef, CilCode.Stobj)]
	public void CorlibStoreIndirect(ElementType type, CilCode code)
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		CorLibTypeSignature? typeToStore = module.CorLibTypeFactory.FromElementType(type);
		Assert.That(typeToStore, Is.Not.Null);
		AssertCorrectStoreIndirect(typeToStore, code);
	}

	[TestCase(ElementType.I1, CilCode.Stind_I1)]
	[TestCase(ElementType.I2, CilCode.Stind_I2)]
	[TestCase(ElementType.I4, CilCode.Stind_I4)]
	[TestCase(ElementType.I8, CilCode.Stind_I8)]
	[TestCase(ElementType.U1, CilCode.Stind_I1)]
	[TestCase(ElementType.U2, CilCode.Stind_I2)]
	[TestCase(ElementType.U4, CilCode.Stind_I4)]
	[TestCase(ElementType.U8, CilCode.Stind_I8)]
	public void EnumStoreIndirect(ElementType type, CilCode code)
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeSignature? typeToStore = CreateEnum(module, type).ToTypeSignature();
		Assert.That(typeToStore, Is.Not.Null);
		AssertCorrectStoreIndirect(typeToStore, code);
	}

	[Test]
	public void PointerStoreIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		PointerTypeSignature typeToStore = module.CorLibTypeFactory.Void.MakePointerType();
		AssertCorrectStoreIndirect(typeToStore, CilCode.Stind_I);
	}

	[Test]
	public void GenericParameterStoreIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		GenericParameterSignature typeToStore = new GenericParameterSignature(module, GenericParameterType.Type, 0);
		AssertCorrectStoreIndirect(typeToStore, CilCode.Stobj);
	}

	[Test]
	public void SzArrayStoreIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeSignature typeToStore = module.CorLibTypeFactory.Int32.MakeSzArrayType();
		AssertCorrectStoreIndirect(typeToStore, CilCode.Stind_Ref);
	}

	[Test]
	public void ClassStoreIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeDefinition type = new TypeDefinition("TestNamespace", "TestName", TypeAttributes.Public, module.CorLibTypeFactory.Object.ToTypeDefOrRef());
		module.TopLevelTypes.Add(type);
		AssertCorrectStoreIndirect(type.ToTypeSignature(), CilCode.Stind_Ref);
	}

	[Test]
	public void StructStoreIndirect()
	{
		ModuleDefinition module = new ModuleDefinition("TestModule");
		TypeDefinition type = new TypeDefinition("TestNamespace", "TestName", TypeAttributes.Public | TypeAttributes.SequentialLayout | TypeAttributes.Sealed, module.DefaultImporter.ImportType(typeof(ValueType)));
		module.TopLevelTypes.Add(type);
		AssertCorrectStoreIndirect(type.ToTypeSignature(), CilCode.Stobj);
	}

	private static void AssertCorrectStoreIndirect(TypeSignature typeToStore, CilCode expectedOpCode)
	{
		MethodDefinition method = new MethodDefinition("Test", MethodAttributes.Public | MethodAttributes.Static, null);
		method.CilMethodBody = new(method);
		CilInstructionCollection instructions = method.CilMethodBody.Instructions;
		instructions.AddStoreIndirect(typeToStore);
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
