using Main.Core;

namespace Main.Helpers;

public static class Utils
{
	public static string ToJson(object obj, bool onlySaveData = false)
	{
		return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
		{
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


	public static bool IsInFront(this Vector2 myFacing, Vector2 dirToTarget, float radius, float fovDeg)
	{
		if (dirToTarget.LengthSquared() > radius * radius) return false;
		if (myFacing == Vector2.Zero) return false;

		var forward = Vector2.Normalize(myFacing);
		var toTarget = Vector2.Normalize(dirToTarget);

		var halfFovRad = MathF.Abs(fovDeg) * MathF.PI / 180f / 2f;
		var cosHalfFov = MathF.Cos(halfFovRad);

		return Vector2.Dot(forward, toTarget) >= cosHalfFov;
	}

	public static Vector2 RotatePoint(this Vector2 point, Vector2 center, float angleDeg)
	{
		float rad = MathF.PI * angleDeg / 180f;
		float cos = MathF.Cos(rad);
		float sin = MathF.Sin(rad);

		float dx = point.X - center.X;
		float dy = point.Y - center.Y;

		float rx = dx * cos - dy * sin;
		float ry = dx * sin + dy * cos;

		return new Vector2(rx + center.X, ry + center.Y);
	}

	public static Vector2[] GetRectangleCorners(Vector2 centerPosition, Vector2 size, float rotationDeg = 0f, Vector2 origin = default)
	{
		if (origin == default) origin = new Vector2(0.5f, 0.5f);

		Vector2 topLeft = centerPosition - new Vector2(origin.X * size.X, origin.Y * size.Y);
		Vector2 topRight = topLeft + new Vector2(size.X, 0f);
		Vector2 bottomRight = topLeft + size;
		Vector2 bottomLeft = topLeft + new Vector2(0f, size.Y);

		Vector2 pivot = centerPosition + (origin - new Vector2(0.5f, 0.5f)) * size;

		if (MathF.Abs(rotationDeg) > 0.0001f)
		{
			topLeft = topLeft.RotatePoint(pivot, rotationDeg);
			topRight = topRight.RotatePoint(pivot, rotationDeg);
			bottomRight = bottomRight.RotatePoint(pivot, rotationDeg);
			bottomLeft = bottomLeft.RotatePoint(pivot, rotationDeg);
		}

		return [topLeft, topRight, bottomRight, bottomLeft]; // clockwise order
	}

	public static List<(Vector2 from, Vector2 to)> GetRectangleEdges(
	Vector2 center, Vector2 size, float rotationDeg = 0f, Vector2 origin = default)
	{
		var corners = GetRectangleCorners(center, size, rotationDeg, origin);
		return new()
		{
			(corners[0], corners[1]),
			(corners[1], corners[2]),
			(corners[2], corners[3]),
			(corners[3], corners[0])
		};
	}

	public static List<Vector2> GetExpandedRectangleCorners(
		Vector2 center, Vector2 size, float rotationDeg, float expandAmount)
	{
		Vector2 expandedSize = size + new Vector2(expandAmount * 2f, expandAmount * 2f);
		return GetRectangleCorners(center, expandedSize, rotationDeg).ToList();
	}

	public static bool IsPointInRotatedRectangle(
		Vector2 point, Vector2 center, Vector2 size, float rotationDeg)
	{
		if (MathF.Abs(rotationDeg) < 0.0001f)
		{
			Rectangle rect = new Rectangle(center - size * 0.5f, size);
			return Raylib.CheckCollisionPointRec(point, rect);
		}

		//invert from world to local space
		Vector2 offset = point - center;
		float rad = -rotationDeg * MathF.PI / 180f;
		float cos = MathF.Cos(rad);
		float sin = MathF.Sin(rad);

		Vector2 local = new Vector2(
			offset.X * cos - offset.Y * sin,
			offset.X * sin + offset.Y * cos
		);

		return MathF.Abs(local.X) <= size.X * 0.5f && MathF.Abs(local.Y) <= size.Y * 0.5f;
	}

	public static bool CheckCollisionCircleRec(
		Vector2 circleCenter,
		float circleRadius,
		Vector2 rectCenter,
		Vector2 rectSize,
		float rotationDeg
	)
	{
		if (circleRadius <= 0f)
			return false;

		if (MathF.Abs(rotationDeg) < 0.0001f)
		{
			var rect = new Rectangle(rectCenter - rectSize * 0.5f, rectSize);
			return Raylib.CheckCollisionCircleRec(circleCenter, circleRadius, rect);
		}

		var offset = circleCenter - rectCenter;
		var rad = -rotationDeg * MathF.PI / 180f;
		var cos = MathF.Cos(rad);
		var sin = MathF.Sin(rad);

		var local = new Vector2(
			offset.X * cos - offset.Y * sin,
			offset.X * sin + offset.Y * cos
		);

		var halfSize = rectSize * 0.5f;

		var closest = new Vector2(
			Math.Clamp(local.X, -halfSize.X, halfSize.X),
			Math.Clamp(local.Y, -halfSize.Y, halfSize.Y)
		);

		var delta = local - closest;
		return delta.LengthSquared() <= (circleRadius * circleRadius);
	}

	public static List<T> GetPage<T>(List<T> source, int pageSize, ref int page, out int totalPages)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be > 0.");

		totalPages = (source.Count + pageSize - 1) / pageSize;
		if (totalPages == 0) totalPages = 1;

		page = Math.Max(1, Math.Min(page, totalPages));

		int start = (page - 1) * pageSize;
		int count = Math.Min(pageSize, source.Count - start);

		return source.Skip(start).Take(count).ToList();
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

	public static Color Lerp(this Color a, Color b, float t)    //raylib-cs implementation is bugged at the moment
	{
		t = Math.Clamp(t, 0f, 1f);

		return new Color(
			(byte)(a.R + t * (b.R - a.R)),
			(byte)(a.G + t * (b.G - a.G)),
			(byte)(a.B + t * (b.B - a.B)),
			(byte)(a.A + t * (b.A - a.A))
		);
	}

	public static Color LerpGradient(this List<Color> colors, float t)
	{
		if (colors == null || colors.Count == 0)
			return default;

		if (colors.Count == 1)
			return colors[0];

		t = Math.Clamp(t, 0f, 1f);

		var cap = colors.Count - 1;
		var scaled = t * cap;
		var i = (int)scaled;
		var frac = scaled - i;

		if (i >= cap)
			return colors[cap];

		return colors[i].Lerp(colors[i + 1], frac);
	}
}