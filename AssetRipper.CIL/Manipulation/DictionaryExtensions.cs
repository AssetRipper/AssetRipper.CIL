namespace AssetRipper.CIL.Manipulation;

internal static class DictionaryExtensions
{
	public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
		where TKey : notnull
		where TValue : new()
	{
		if (!dictionary.TryGetValue(key, out TValue? value))
		{
			value = new TValue();
			dictionary[key] = value;
		}
		return value;
	}
}
