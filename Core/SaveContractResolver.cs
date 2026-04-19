using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Main.Core;

//entire levels are saved as JSON file (entire World.cs), then any progress will be saved and loaded in separate save progress JSON file using SaveDataAttribute properties.
//then once loaded, that small loaded data will override properties from original level JSON data to make it efficient and automatic
//also known as "Bethesday-style" or delta-saving

[AttributeUsage(AttributeTargets.Property)]
public class LevelDataAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class SaveDataAttribute : Attribute { }

public class SaveContractResolver : DefaultContractResolver
{
	private readonly bool _onlySaveData;
	public SaveContractResolver(bool onlySaveData) => _onlySaveData = onlySaveData;

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		JsonProperty property = base.CreateProperty(member, memberSerialization);

		var isLevelData = member.GetCustomAttribute<LevelDataAttribute>() != null;
		var isSaveData = member.GetCustomAttribute<SaveDataAttribute>() != null;

		if (_onlySaveData && isLevelData && !isSaveData)
			property.ShouldSerialize = _ => false;

		return property;
	}
}