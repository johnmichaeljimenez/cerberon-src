using System.Globalization;
using System.Reflection;

public static class TsvParser
{
	public static List<T> Parse<T>(string content) where T : new() //TODO: remove textreader below and use this directly
	{
		if (content == null) throw new ArgumentNullException(nameof(content));

		using var reader = new StringReader(content);
		return Parse<T>(reader);
	}

	public static List<T> Parse<T>(TextReader reader) where T : new()
	{
		var result = new List<T>();

		string headerLine = reader.ReadLine()
			?? throw new InvalidDataException("TSV is empty.");

		var headers = headerLine.Split('\t') //TODO: add support for quoted strings with tab
								.Select(h => h.Trim())
								.ToArray();

		var propMap = typeof(T)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(p => p.CanWrite)
			.ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

		var columnInfos = new (int Index, PropertyInfo Prop)[headers.Length];
		for (int i = 0; i < headers.Length; i++)
		{
			if (propMap.TryGetValue(headers[i], out var prop))
				columnInfos[i] = (i, prop);
			else
				columnInfos[i] = (i, null);
		}

		string line;
		while ((line = reader.ReadLine()) != null)
		{
			if (string.IsNullOrWhiteSpace(line)) continue;

			var fields = line.Split('\t');
			var obj = new T();

			foreach (var (idx, prop) in columnInfos)
			{
				if (prop == null) continue;

				string raw = idx < fields.Length ? fields[idx].Trim() : string.Empty;
				if (string.IsNullOrEmpty(raw)) continue; //let the default values to be used if column is blank for this row

				object value = ConvertValue(raw, prop.PropertyType);
				prop.SetValue(obj, value);
			}

			result.Add(obj);
		}

		return result;
	}

	private static object ConvertValue(string raw, Type targetType) //let outsider handle error
	{
		if (targetType == typeof(string))
			return raw;

		var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

		if (underlying.IsEnum)
			return Enum.Parse(underlying, raw, ignoreCase: true);

		if (underlying == typeof(Vector2))
		{
			var split = raw.Replace(" ", "").Split(","); //ex: 10,4 or 10, 4

			return new Vector2(
				float.Parse(split[0], CultureInfo.InvariantCulture),
				float.Parse(split[1], CultureInfo.InvariantCulture)
			);
		}

		if (underlying == typeof(bool))
		{
			if (raw.Equals("1"))
				return true;
			else if (raw.Equals("0"))
				return false;

			return bool.Parse(raw);
		}

		// numeric
		return Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
	}
}