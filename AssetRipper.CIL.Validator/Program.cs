using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;

namespace AssetRipper.CIL.Validator;

internal class Program
{
	static void Main(string[] args)
	{
		ModuleDefinition module1 = ModuleDefinition.FromFile(args[0]);
		ModuleDefinition module2 = ModuleDefinition.FromFile(args[1]);

		List<TypeDefinition> typesMissingFrom1 = new();
		List<TypeDefinition> typesMissingFrom2 = new();
		Dictionary<TypeDefinition, TypeDefinition> type1ToType2 = new();
		MatchTypes(module1, module2, typesMissingFrom1, typesMissingFrom2, type1ToType2);

		List<MethodDefinition> methodsMissingFrom1 = new();
		List<MethodDefinition> methodsMissingFrom2 = new();
		Dictionary<MethodDefinition, MethodDefinition> method1ToMethod2 = new();
		foreach ((TypeDefinition type1, TypeDefinition type2) in type1ToType2)
		{
			MatchNameAndSignature(type1.Methods, type2.Methods, methodsMissingFrom1, methodsMissingFrom2, method1ToMethod2, m => m.Signature!, SignatureComparer.Default);
		}

		List<(MethodDefinition, MethodDefinition)> differentMethods = new();
		foreach ((MethodDefinition method1, MethodDefinition method2) in method1ToMethod2)
		{
			if (method1.CilMethodBody is null)
			{
				if (method2.CilMethodBody is not null)
				{
					differentMethods.Add((method1, method2));
				}
				continue;
			}
			else if (method2.CilMethodBody is null)
			{
				differentMethods.Add((method1, method2));
				continue;
			}

			method1.CilMethodBody.Instructions.ExpandMacros();
			method2.CilMethodBody.Instructions.ExpandMacros();
			if (!CilInstructionCollectionEquality.Equals(method1.CilMethodBody, method2.CilMethodBody))
			{
				CilInstructionCollectionEquality.Equals(method1.CilMethodBody, method2.CilMethodBody);
				differentMethods.Add((method1, method2));
			}
		}

		List<FieldDefinition> fieldsMissingFrom1 = new();
		List<FieldDefinition> fieldsMissingFrom2 = new();
		Dictionary<FieldDefinition, FieldDefinition> field1ToField2 = new();
		foreach ((TypeDefinition type1, TypeDefinition type2) in type1ToType2)
		{
			MatchName(type1.Fields, type2.Fields, fieldsMissingFrom1, fieldsMissingFrom2, field1ToField2);
		}

		List<(FieldDefinition, FieldDefinition)> differentFields = new();
		foreach ((FieldDefinition field1, FieldDefinition field2) in field1ToField2)
		{
			if (!SignatureComparer.Default.Equals(field1.Signature, field2.Signature))
			{
				differentFields.Add((field1, field2));
			}
		}

		List<PropertyDefinition> propertiesMissingFrom1 = new();
		List<PropertyDefinition> propertiesMissingFrom2 = new();
		Dictionary<PropertyDefinition, PropertyDefinition> property1ToProperty2 = new();
		foreach ((TypeDefinition type1, TypeDefinition type2) in type1ToType2)
		{
			MatchNameAndSignature(type1.Properties, type2.Properties, propertiesMissingFrom1, propertiesMissingFrom2, property1ToProperty2, p => p.Signature!, SignatureComparer.Default);
		}

		List<EventDefinition> eventsMissingFrom1 = new();
		List<EventDefinition> eventsMissingFrom2 = new();
		Dictionary<EventDefinition, EventDefinition> event1ToEvent2 = new();
		foreach ((TypeDefinition type1, TypeDefinition type2) in type1ToType2)
		{
			MatchName(type1.Events, type2.Events, eventsMissingFrom1, eventsMissingFrom2, event1ToEvent2);
		}

		List<(EventDefinition, EventDefinition)> differentEvents = new();
		foreach ((EventDefinition event1, EventDefinition event2) in event1ToEvent2)
		{
			if (!SignatureComparer.Default.Equals(event1.EventType, event2.EventType))
			{
				differentEvents.Add((event1, event2));
			}
		}

		Console.WriteLine($"Types missing from module 1: {typesMissingFrom1.Count}");
		Console.WriteLine($"Types missing from module 2: {typesMissingFrom2.Count}");
		Console.WriteLine($"Types matched: {type1ToType2.Count}");
		Console.WriteLine();
		Console.WriteLine($"Methods missing from module 1: {methodsMissingFrom1.Count}");
		Console.WriteLine($"Methods missing from module 2: {methodsMissingFrom2.Count}");
		Console.WriteLine($"Methods matched: {method1ToMethod2.Count}");
		Console.WriteLine($"Different methods: {differentMethods.Count}");
		Console.WriteLine();
		Console.WriteLine($"Fields missing from module 1: {fieldsMissingFrom1.Count}");
		Console.WriteLine($"Fields missing from module 2: {fieldsMissingFrom2.Count}");
		Console.WriteLine($"Fields matched: {field1ToField2.Count}");
		Console.WriteLine($"Different fields: {differentFields.Count}");
		Console.WriteLine();
		Console.WriteLine($"Properties missing from module 1: {propertiesMissingFrom1.Count}");
		Console.WriteLine($"Properties missing from module 2: {propertiesMissingFrom2.Count}");
		Console.WriteLine($"Properties matched: {property1ToProperty2.Count}");
		Console.WriteLine();
		Console.WriteLine($"Events missing from module 1: {eventsMissingFrom1.Count}");
		Console.WriteLine($"Events missing from module 2: {eventsMissingFrom2.Count}");
		Console.WriteLine($"Events matched: {event1ToEvent2.Count}");
		Console.WriteLine($"Different events: {differentEvents.Count}");
	}

	private static void MatchName<T>(IList<T> list1, IList<T> list2, List<T> missingFrom1, List<T> missingFrom2, Dictionary<T, T> value1ToValue2)
		where T : class, INameProvider
	{
		for (int i = 0; i < list2.Count; i++)
		{
			T value2 = list2[i];
			if (i < list1.Count && list1[i].Name == value2.Name)
			{
				value1ToValue2.Add(list1[i], value2);
				continue;
			}

			T? value1 = list1.FirstOrDefault(v => v.Name == value2.Name);
			if (value1 is null)
			{
				missingFrom1.Add(value2);
			}
			else
			{
				value1ToValue2.Add(value1, value2);
			}
		}

		foreach (T value1 in list1)
		{
			if (!value1ToValue2.ContainsKey(value1))
			{
				missingFrom2.Add(value1);
			}
		}
	}

	private static void MatchNameAndSignature<T, TSignature>(IList<T> list1, IList<T> list2, List<T> missingFrom1, List<T> missingFrom2, Dictionary<T, T> value1ToValue2, Func<T, TSignature> getSignature, IEqualityComparer<TSignature> signatureComparer)
		where T : class, INameProvider
	{
		for (int i = 0; i < list2.Count; i++)
		{
			T value2 = list2[i];
			if (i < list1.Count && list1[i].Name == value2.Name && SignatureEquals(list1[i], value2, getSignature, signatureComparer))
			{
				value1ToValue2.Add(list1[i], value2);
				continue;
			}

			T? value1 = list1.FirstOrDefault(v => v.Name == value2.Name && SignatureEquals(v, value2, getSignature, signatureComparer));
			if (value1 is null)
			{
				missingFrom1.Add(value2);
			}
			else
			{
				value1ToValue2.Add(value1, value2);
			}
		}

		foreach (T value1 in list1)
		{
			if (!value1ToValue2.ContainsKey(value1))
			{
				missingFrom2.Add(value1);
			}
		}

		static bool SignatureEquals(T value1, T value2, Func<T, TSignature> getSignature, IEqualityComparer<TSignature> signatureComparer)
		{
			return signatureComparer.Equals(getSignature(value1), getSignature(value2));
		}
	}

	private static void MatchTypes(ModuleDefinition module1, ModuleDefinition module2, List<TypeDefinition> typesMissingFrom1, List<TypeDefinition> typesMissingFrom2, Dictionary<TypeDefinition, TypeDefinition> type1ToType2)
	{
		foreach (TypeDefinition type2 in module2.TopLevelTypes)
		{
			TypeDefinition? type1 = module1.TopLevelTypes.FirstOrDefault(t => SignatureComparer.Default.Equals(t, type2));
			if (type1 is null)
			{
				typesMissingFrom1.Add(type2);
			}
			else
			{
				type1ToType2.Add(type1, type2);
				MatchNestedTypes(type1, type2, typesMissingFrom1, typesMissingFrom2, type1ToType2);
			}
		}

		foreach (TypeDefinition type1 in module1.TopLevelTypes)
		{
			if (!type1ToType2.ContainsKey(type1))
			{
				typesMissingFrom2.Add(type1);
			}
		}

		static void MatchNestedTypes(TypeDefinition type1, TypeDefinition type2, List<TypeDefinition> typesMissingFrom1, List<TypeDefinition> typesMissingFrom2, Dictionary<TypeDefinition, TypeDefinition> type1ToType2)
		{
			foreach (TypeDefinition nestedType2 in type2.NestedTypes)
			{
				TypeDefinition? nestedType1 = type1.NestedTypes.FirstOrDefault(t => t.Name == nestedType2.Name);
				if (nestedType1 is null)
				{
					typesMissingFrom1.Add(nestedType2);
				}
				else
				{
					type1ToType2.Add(nestedType1, nestedType2);
					MatchNestedTypes(nestedType1, nestedType2, typesMissingFrom1, typesMissingFrom2, type1ToType2);
				}
			}

			foreach (TypeDefinition nestedType1 in type1.NestedTypes)
			{
				if (!type1ToType2.ContainsKey(nestedType1))
				{
					typesMissingFrom2.Add(nestedType1);
				}
			}
		}
	}
}
