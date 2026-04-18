using Main.Core;

namespace Main.Effects;

public class Light
{
	public Sprite Sprite { get; set; }
	public Vector2 Position { get; set; }
	public Color Color { get; set; }
	public float Rotation { get; set; }
	public Vector2 Origin { get; set; }
	public float Scale { get; set; }
	public bool Enabled { get; set; }

	public Light(Sprite sprite, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null)
	{
		var org = origin ?? new(0.5f, 0.5f);

		Sprite = sprite;
		Position = position;
		Color = color;
		Rotation = rotation;
		Origin = org;
		Scale = scale;
		Enabled = enabled;
	}
}

public static class LightingSystem //simple additive lighting only, no shadows or any fancy stuff for now
{
	public static RenderTexture2D RenderTexture { get; private set; }

	private static readonly List<Light> lights = new();

	public static void Init(int width, int height)
	{
		RenderTexture = Raylib.LoadRenderTexture(width, height);
		Raylib.SetTextureFilter(RenderTexture.Texture, TextureFilter.Bilinear);

		Raylib.SetTextureFilter(AssetManager.GetSprite("light").Texture, TextureFilter.Bilinear);
		Raylib.SetTextureFilter(AssetManager.GetSprite("flashlight").Texture, TextureFilter.Bilinear);
	}

	public static Light AddLight(Sprite sprite, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null)
	{
		var light = new Light(sprite, position, color, rotation, scale, enabled, origin);
		lights.Add(light);

		return light;
	}

	public static void RemoveLight(Light light)
	{
		lights.Remove(light);
	}

	public static void Draw()
	{
		if (lights.Count == 0)
			return;

		Raylib.BeginTextureMode(RenderTexture);
		Raylib.BeginMode2D(Game.Instance.Camera.Camera);
		Raylib.BeginBlendMode(BlendMode.Additive);
		Raylib.ClearBackground(Color.Black);

		foreach (var i in lights)
		{
			if (!i.Enabled)
				continue;

			i.Sprite.Draw(i.Position, tint: i.Color, rotation: i.Rotation, origin: i.Origin, scale: i.Scale);
		}

		Raylib.EndBlendMode();
		Raylib.EndMode2D();
		Raylib.EndTextureMode();
	}

	public static void Dispose()
	{
		Clear();
		Raylib.UnloadRenderTexture(RenderTexture);
	}

	public static void Clear()
	{
		lights.Clear();
	}
}