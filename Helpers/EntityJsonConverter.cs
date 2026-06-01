using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Level;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Cerberon.Helpers;

public class EntityJsonConverter : JsonConverter<BaseEntity>
{
	public override BaseEntity? ReadJson(JsonReader reader, Type objectType, BaseEntity? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null) return null;

		JObject jo = JObject.Load(reader);

		string? typeStr = jo.Value<string>("Type");
		if (string.IsNullOrEmpty(typeStr))
			throw new JsonSerializationException("Entity is missing the 'Type' property.");

		Type? entityType = World.GetRegisteredEntityType(typeStr);
		if (entityType == null)
			throw new JsonSerializationException($"Unknown entity type: {typeStr}");

		jo.Remove("Type");

		using (var jr = jo.CreateReader())
		{
			var innerSettings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Populate,
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = serializer.ContractResolver,
				Converters = { new StringEnumConverter() }	//let this specific converter to use string-based enum instead of int
			};

			return (BaseEntity)JsonSerializer.Create(innerSettings).Deserialize(jr, entityType)!;
		}
	}

	public override void WriteJson(JsonWriter writer, BaseEntity? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}

		var tempSettings = new JsonSerializerSettings
		{
			ContractResolver = serializer.ContractResolver,
			NullValueHandling = serializer.NullValueHandling,
			DefaultValueHandling = serializer.DefaultValueHandling,
			TypeNameHandling = TypeNameHandling.None,
			Converters = { new StringEnumConverter() }
		};
		var tempSerializer = JsonSerializer.Create(tempSettings);

		JObject jo = JObject.FromObject(value, tempSerializer);
		jo["Type"] = value.GetType().Name;

		jo.WriteTo(writer);
	}
}