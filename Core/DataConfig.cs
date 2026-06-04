using System.Reflection;
using Cerberon.Core;
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
	public object? DefaultValue { get; }

	public ConfigEntry(MemberInfo member, string key, object? defaultValue)
	{
		Member = member;
		Key = key;
		ValueType = member is FieldInfo fi ? fi.FieldType : ((PropertyInfo)member).PropertyType;
		DefaultValue = defaultValue;
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

		LoadFromDefault();
	}

	private static void ResetToDefaults()
	{
		foreach (var entry in _cache.Values)
		{
			entry.SetValue(entry.DefaultValue);
			Log.Send($"'{entry.Key}' reset to default: {entry.DefaultValue ?? "null"}");
		}
	}

	private static void TryAddMember(MemberInfo member)
	{
		var attr = member.GetCustomAttribute<DataConfigAttribute>();
		if (attr == null) return;

		var key = attr.Key ?? $"{member.DeclaringType?.Name}.{member.Name}";

		//get the attribute value first, otherwise just take the default set value
		object? defaultValue = attr.DefaultValue;
		if (defaultValue == null)
			defaultValue = GetCurrentValue(member);

		var entry = new ConfigEntry(member, key, defaultValue);
		_cache[key] = entry;

		if (attr.DefaultValue != null)
		{
			entry.SetValue(attr.DefaultValue);
		}
	}

	private static object? GetCurrentValue(MemberInfo member)
	{
		try
		{
			if (member is FieldInfo field)
			{
				return field.GetValue(null);
			}
			else if (member is PropertyInfo prop && prop.CanRead)
			{
				return prop.GetValue(null);
			}
		}
		catch (Exception ex)
		{
			Log.Send($"Failed to read current value for config member '{member.Name}': {ex.Message}");
		}

		return null;
	}

	public static void LoadFromDefault()
	{
		ResetToDefaults();

		var path = "Assets/config.json";
		if (!File.Exists(path))
			return;

		try
		{
			LoadFromJson(File.ReadAllText(path), false);
		}
		catch (JsonReaderException ex)
		{
			Log.Send($"Invalid JSON: {ex.Message}");
		}
	}

	public static void LoadFromJson(string jsonString, bool resetToDefaults = true)
	{
		if (string.IsNullOrWhiteSpace(jsonString))
		{
			if (resetToDefaults)
				ResetToDefaults();
			return;
		}

		try
		{
			var data = JsonConvert.DeserializeObject<Dictionary<string, object?>>(jsonString);
			LoadData(data, resetToDefaults);
		}
		catch (JsonReaderException ex)
		{
			Log.Send($"Invalid JSON: {ex.Message}");
		}
	}

	public static void LoadData(Dictionary<string, object?>? data, bool resetToDefaults = true)
	{
		if (resetToDefaults)
		{
			ResetToDefaults();
		}

		if (data == null || data.Count == 0) return;

		foreach (var kvp in data)
		{
			var key = kvp.Key;
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