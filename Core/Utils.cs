namespace Main.Core;

public static class Utils
{
	//how do I either automatically fully allow or deny properties in a class to be serialized?
	public static JsonSerializerSettings settings = new()
	{
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		TypeNameHandling = TypeNameHandling.Auto,
		NullValueHandling = NullValueHandling.Ignore
	};

	public static void FromJson<T>(this T obj, string json) where T : class
	{
		JsonConvert.PopulateObject(json, obj, settings);
	}

	public static string ToJson<T>(this T obj)
	{
		return JsonConvert.SerializeObject(obj, settings);
	}
}