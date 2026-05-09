using System.Reflection;
using Main.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class DataConfigAttribute : Attribute
{
	public string? Key { get; }
	public object? DefaultValue { get; }

	public DataConfigAttribute(object defaultValue)
	{
		Key = null;
		DefaultValue = defaultValue;
	}

	public DataConfigAttribute(string? key = null, object? defaultValue = null)
	{
		Key = key;
		DefaultValue = defaultValue;
	}
}

public class ConfigEntry
{
	public MemberInfo Member { get; }
	public string Key { get; }
	public Type ValueType { get; }

	public ConfigEntry(MemberInfo member, string key)
	{
		Member = member;
		Key = key;
		ValueType = member is FieldInfo fi ? fi.FieldType : ((PropertyInfo)member).PropertyType;
	}

	public void SetValue(object? value)
	{
		if (Member is FieldInfo field)
			field.SetValue(null, value);
		else if (Member is PropertyInfo prop)
			prop.SetValue(null, value);
	}
}

public static class DataConfigManager
{
	private static readonly Dictionary<string, ConfigEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

	public static void Initialize()
	{
		var types = Assembly.GetExecutingAssembly().GetTypes();
		_cache.Clear();

		foreach (var type in types)
		{
			foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				TryAddMember(field);
			}

			foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (prop.CanWrite)
					TryAddMember(prop);
			}
		}
	}

	private static void TryAddMember(MemberInfo member)
	{
		var attr = member.GetCustomAttribute<DataConfigAttribute>();
		if (attr == null) return;

		string key = attr.Key ?? $"{member.DeclaringType?.Name}.{member.Name}";

		var entry = new ConfigEntry(member, key);
		_cache[key] = entry;

		if (attr.DefaultValue != null)
		{
			entry.SetValue(attr.DefaultValue);
			Log.Send($"'{key}' value reset: {attr.DefaultValue}");
		}
	}

	public static void LoadFromJson(string jsonString)
	{
		if (string.IsNullOrWhiteSpace(jsonString)) return;

		try
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonString);
			LoadData(data);
		}
		catch (JsonReaderException ex)
		{
			Log.Send($"Invalid JSON: {ex.Message}");
		}
	}

	public static void LoadData(Dictionary<string, object?>? data)
	{
		if (data == null || data.Count == 0) return;

		foreach (var kvp in data)
		{
			string key = kvp.Key;
			if (string.IsNullOrEmpty(key)) continue;

			if (_cache.TryGetValue(key, out var entry))
			{
				try
				{
					object? value = kvp.Value;

					if (value is JToken token)
					{
						value = token.ToObject(entry.ValueType);
					}

					entry.SetValue(value);
					Log.Send($"'{key}' value override: {value}");
				}
				catch (Exception ex)
				{
					Log.Send($"Failed to set config '{key}': {ex.Message}");
				}
			}
		}
	}
}