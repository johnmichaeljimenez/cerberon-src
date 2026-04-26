using Main.Core;

namespace Main.Helpers;

public static class Utils
{
	public static string ToJson(object obj, bool onlySaveData = false)
	{
		return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
		{
			ContractResolver = new SaveContractResolver(onlySaveData),
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.None,
			NullValueHandling = NullValueHandling.Ignore,
			Converters = { new EntityJsonConverter() }
		});
	}

	public static T FromJson<T>(this string json)
	{
		var settings = new JsonSerializerSettings()
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Populate,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			Converters = { new EntityJsonConverter() }
		};

		return JsonConvert.DeserializeObject<T>(json, settings);
	}

	public static void FromJsonPopulate(this object obj, string json)
	{
		JsonConvert.PopulateObject(json, obj, new());
	}

	public static float ToDirection(this Vector2 delta)
	{
		var angleRadians = (float)Math.Atan2(delta.Y, delta.X);
		var angleDegrees = angleRadians * 180 / (float)Math.PI;
		return (angleDegrees + 360) % 360;
	}

	public static float ToDirection(this Vector2 from, Vector2 to)
	{
		var delta = to - from;
		return ToDirection(delta);
	}

	public static void DrawLineEx(Vector2 from, Vector2 to, Vector2 mid, Vector2 normal, Color color)
	{
		Raylib.DrawLineV(from, to, color);

		normal = new Vector2(normal.X * 0.5f, normal.Y * 0.5f);
		Raylib.DrawLineV(mid, new Vector2(mid.X + normal.X, mid.Y + normal.Y), color);
	}

	public static bool Countdown(ref float t, float dt) //i just feel like using ref
	{
		t -= dt;
		if (t <= 0)
		{
			t = 0;
			return true;
		}

		return false;
	}

	public static TDisposable AddTo<TDisposable>(this TDisposable disposable, List<IDisposable> bag)
		where TDisposable : IDisposable
	{
		bag.Add(disposable);
		return disposable;
	}

	public static Rectangle Expand(this Rectangle rect, float r)
	{
		return new Rectangle
		(
			new Vector2(rect.Position.X - r,
						rect.Position.Y - r),
			new Vector2(rect.Size.X + 2 * r,
						rect.Size.Y + 2 * r)
		);
	}

	public static List<Vector2> ToVector2List(this Rectangle rect)
	{
		return new List<Vector2>(){
			new Vector2(rect.Position.X, rect.Position.Y),
			new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y),
			new Vector2(rect.Position.X + rect.Size.X, rect.Position.Y + rect.Size.Y),
			new Vector2(rect.Position.X, rect.Position.Y + rect.Size.Y)
		};
	}

	public static Rectangle Enclose(Vector2 from, Vector2 to)
	{
		var x = Math.Min(from.X, to.X);
		var y = Math.Min(from.Y, to.Y);
		var width = Math.Abs(from.X - to.X);
		var height = Math.Abs(from.Y - to.Y);

		return new Rectangle(x, y, width, height);
	}

	public static List<Rectangle> GetChunkRectangles(this Rectangle worldArea, float chunkWidth, float chunkHeight)
	{
		var chunks = new List<Rectangle>();

		for (var y = worldArea.Y; y < worldArea.Y + worldArea.Height; y += chunkHeight)
		{
			for (var x = worldArea.X; x < worldArea.X + worldArea.Width; x += chunkWidth)
			{
				var w = Math.Min(chunkWidth, worldArea.X + worldArea.Width - x);
				var h = Math.Min(chunkHeight, worldArea.Y + worldArea.Height - y);

				chunks.Add(new Rectangle(x, y, w, h));
			}
		}

		return chunks;
	}
}

public static class Colors
{
	//i don't know why ray chose his RGB colors as the official one, it breaks a lot of shader effects
	public static readonly Color RED = new Color(255, 0, 0);
	public static readonly Color GREEN = new Color(0, 255, 0);
	public static readonly Color BLUE = new Color(0, 0, 255);
	public static readonly Color WHITE = new Color(255, 255, 255);
	public static readonly Color YELLOW = new Color(255, 255, 0);

	public static Color Multiply(this Color c, float r = 1.0f, float g = 1.0f, float b = 1.0f)
	{
		return new()
		{
			R = (byte)(c.R * r),
			G = (byte)(c.G * g),
			B = (byte)(c.B * b),
			A = c.A
		};
	}

	public static Color Multiply(this Color c, float i = 1.0f)
	{
		return new()
		{
			R = (byte)(c.R * i),
			G = (byte)(c.G * i),
			B = (byte)(c.B * i),
			A = c.A
		};
	}

	public static Color Fade(this Color c, float alpha)
	{
		return new(c.R, c.G, c.B, alpha);
	}

	public static Color Value(this Color c, float amt)
	{
		return new((byte)((float)c.R * amt), (byte)((float)c.G * amt), (byte)((float)c.B * amt), c.A);
	}
}