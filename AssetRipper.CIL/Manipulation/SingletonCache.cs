namespace AssetRipper.CIL.Manipulation;

internal static class SingletonCache<T> where T : new()
{
	public static T Instance { get; } = new T();
}
