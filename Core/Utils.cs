namespace Main.Core;

public static class Utils
{
	public static string ToJson(object obj, bool onlySaveData = false)
	{
		return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
		{
			ContractResolver = new SaveContractResolver(onlySaveData),
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Auto, //temporary (I know this is dangerous)
			NullValueHandling = NullValueHandling.Ignore
		});
	}

	public static void FromJson(this object obj, string json)
	{
		JsonConvert.PopulateObject(json, obj, new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto //temporary (I know this is dangerous)
		});
	}

	public static float ToDirection(this Vector2 from, Vector2 to)
	{
		var delta = to - from;
		var angleRadians = (float)Math.Atan2(delta.Y, delta.X);
		var angleDegrees = angleRadians * 180 / (float)Math.PI;
		return (angleDegrees + 360) % 360;
	}
}