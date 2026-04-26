using Main.Gameplay;
using Main.Gameplay.Entities;
using Newtonsoft.Json.Linq;

namespace Main.Helpers;

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

		BaseEntity entity = (BaseEntity)Activator.CreateInstance(entityType)!;

		using (var jr = jo.CreateReader())
		{
			serializer.Populate(jr, entity);
		}

		return entity;
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
			TypeNameHandling = TypeNameHandling.None
		};
		var tempSerializer = JsonSerializer.Create(tempSettings);

		JObject jo = JObject.FromObject(value, tempSerializer);
		jo["Type"] = value.GetType().Name;

		jo.WriteTo(writer);
	}
}