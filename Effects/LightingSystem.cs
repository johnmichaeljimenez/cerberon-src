using Main.Core;
using Main.Helpers;

namespace Main.Effects;

public class Shadow
{
	public Rectangle Bounds => new Rectangle(Position, Size);
	public Vector2 Position;
	public Vector2 Size;

	public readonly Vector2[] Points;

	public Shadow(Vector2 position, Vector2 size)
	{
		Position = position;
		Size = size;

		Points = Bounds.ToVector2List().ToArray(); //clockwise
	}

	public void DrawShadow(Light light)
	{
		const float farDistance = 100;
		const float sideExtrude = 80;

		Raylib.DrawRectangleRec(Bounds, Color.Black);

		for (int i = 0; i < Points.Length; i++)
		{
			var from = Points[i];
			var to = Points[(i + 1) % Points.Length];

			var edge = to - from;
			var normal = Raymath.Vector2Normalize(new Vector2(-edge.Y, edge.X));

			var midpoint = (from + to) / 2f;
			var d = light.Position - midpoint;
			bool visible = Raymath.Vector2DotProduct(normal, d) > 0f;

			if (!visible)
				continue;

			var dirFrom = Raymath.Vector2Normalize(from - light.Position);
			var dirTo = Raymath.Vector2Normalize(to - light.Position);

			var sFrom = from + dirFrom * farDistance;
			var sTo = to + dirTo * farDistance;

			var sFrom2 = sFrom - normal * sideExtrude;
			var sTo2 = sTo - normal * sideExtrude;

			Vector2[] stripPoints =
			[
				from,
				to,
				sFrom,
				sTo,
				sFrom2,
				sTo2
			];

			Raylib.DrawTriangleStrip(stripPoints, 6, Color.Black);
		}
	}
}

public class Light : IDisposable
{
	public enum VisionEffects
	{
		Light,
		VisionOnly,
	}

	public Sprite Sprite { get; set; }
	public Vector2 Position { get; set; }
	public Color Color { get; set; }
	public float Rotation { get; set; }
	public Vector2 Origin { get; set; }
	public float Scale { get; set; }
	public bool Enabled { get; set; }
	public VisionEffects VisionEffect { get; set; }

	public bool CastShadows { get; set; }
	public RenderTexture2D? ShadowRenderTexture { get; private set; }
	public Camera2D? ShadowCamera { get; private set; }
	private const float SHADOW_MAP_RESOLUTION = 256f;

	public Light(Sprite sprite, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null, bool castShadows = false, VisionEffects visionEffect = VisionEffects.Light)
	{
		var org = origin ?? new(0.5f, 0.5f);

		Sprite = sprite;
		Position = position;
		Color = color;
		Rotation = rotation;
		Origin = org;
		Scale = scale;
		Enabled = enabled;
		VisionEffect = visionEffect;
		CastShadows = castShadows;

		if (CastShadows)
		{
			int rtSize = (int)SHADOW_MAP_RESOLUTION; //TODO: use world unity dynamic resolution (bigger light = bigger RT)

			ShadowRenderTexture = Raylib.LoadRenderTexture(rtSize, rtSize);
			Raylib.SetTextureFilter(ShadowRenderTexture.Value.Texture, TextureFilter.Bilinear);

			float spriteWorldDiameter = sprite.UnitSize.X * scale * 2;

			ShadowCamera = new Camera2D
			{
				Target = position,
				Offset = new Vector2(rtSize / 2f, rtSize / 2f),
				Zoom = SHADOW_MAP_RESOLUTION / spriteWorldDiameter,
				Rotation = 0f
			};
		}
	}

	public void Dispose()
	{
		if (ShadowRenderTexture.HasValue)
			Raylib.UnloadRenderTexture(ShadowRenderTexture.Value);
	}
}

public static class LightingSystem
{
	public static RenderTexture2D LightingRenderTexture { get; private set; }
	public static RenderTexture2D VisionRenderTexture { get; private set; }
	public static Color AmbientLightColor { get; set; }
	public const float SCALE = 2.0f;

	private static readonly List<Light> lights = new();
	private static readonly List<Shadow> shadows = new();
	private static readonly List<Light> visionLights = new();

	public static void Init(int width, int height)
	{
		LightingRenderTexture = Raylib.LoadRenderTexture((int)(width / SCALE), (int)(height / SCALE));
		Raylib.SetTextureFilter(LightingRenderTexture.Texture, TextureFilter.Bilinear);

		VisionRenderTexture = Raylib.LoadRenderTexture((int)(width / SCALE), (int)(height / SCALE));
		Raylib.SetTextureFilter(VisionRenderTexture.Texture, TextureFilter.Bilinear);

		Raylib.SetTextureFilter(AssetManager.GetSprite("light").Texture, TextureFilter.Bilinear);
		Raylib.SetTextureFilter(AssetManager.GetSprite("flashlight").Texture, TextureFilter.Bilinear);
	}

	public static Light AddLight(Sprite sprite, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null, bool castShadows = false, Light.VisionEffects visionEffect = Light.VisionEffects.Light)
	{
		var light = new Light(sprite, position, color, rotation, scale, enabled, origin, castShadows);

		if (visionEffect == Light.VisionEffects.Light)
			lights.Add(light);
		else if (visionEffect == Light.VisionEffects.VisionOnly)
			visionLights.Add(light);

		return light;
	}

	public static void RemoveLight(Light light)
	{
		light.Dispose();

		if (lights.Contains(light))
			lights.Remove(light);

		if (visionLights.Contains(light))
			visionLights.Remove(light);
	}

	public static Shadow AddShadow(Vector2 position, Vector2 size)
	{
		var shadow = new Shadow(position, size);
		shadows.Add(shadow);

		return shadow;
	}

	public static void RemoveShadow(Shadow shadow)
	{
		shadows.Remove(shadow);
	}

	private static void DrawLights(Camera2D cam, RenderTexture2D tex, List<Light> l, Color bgColor)
	{
		foreach (var i in l)
		{
			if (!i.Enabled)
				continue;

			if (!i.CastShadows)
				continue;

			//nested render texture drawings are not allowed, so shadows first
			if (i.ShadowRenderTexture.HasValue && i.ShadowCamera.HasValue) //TODO: add hasUpdated flag for light and shadow
			{
				var rt = i.ShadowRenderTexture.Value;
				var lightCam = i.ShadowCamera.Value;

				lightCam.Target = i.Position;

				Raylib.BeginTextureMode(rt);
				Raylib.ClearBackground(Color.Black);
				Raylib.BeginMode2D(lightCam);

				i.Sprite.Draw(i.Position, tint: i.Color, rotation: i.Rotation, origin: i.Origin, scale: i.Scale);

				foreach (var shadow in shadows)
				{
					shadow.DrawShadow(i);
				}

				Raylib.EndMode2D();
				Raylib.EndTextureMode();
			}
		}

		Raylib.BeginTextureMode(tex);
		Raylib.BeginMode2D(cam);
		Raylib.BeginBlendMode(BlendMode.Additive);
		Raylib.ClearBackground(bgColor);

		foreach (var i in l)
		{
			if (!i.Enabled)
				continue;

			if (!i.CastShadows)
			{
				i.Sprite.Draw(i.Position, tint: i.Color, rotation: i.Rotation, origin: i.Origin, scale: i.Scale);
				continue;
			}

			if (i.ShadowRenderTexture.HasValue && i.ShadowCamera.HasValue)
			{
				var rt = i.ShadowRenderTexture.Value;
				var lightCam = i.ShadowCamera.Value;

				float worldSize = (float)rt.Texture.Width / lightCam.Zoom;
				Rectangle dest = new Rectangle(
					i.Position.X - worldSize / 2f,
					i.Position.Y - worldSize / 2f,
					worldSize,
					worldSize
				);

				Rectangle src = new Rectangle(0, 0, rt.Texture.Width, -rt.Texture.Height);

				Raylib.DrawTexturePro(rt.Texture, src, dest, Vector2.Zero, 0f, Color.White);
			}
		}

		Raylib.EndBlendMode();
		Raylib.EndMode2D();
		Raylib.EndTextureMode();
	}

	public static void Draw()
	{
		if (lights.Count == 0)
			return;

		var cam = Game.Instance.Camera.Camera;
		cam.Offset = new Vector2(LightingRenderTexture.Texture.Width, LightingRenderTexture.Texture.Height) / 2f;
		cam.Zoom /= SCALE; //optimization and it ironically makes the lighting look better (non-HD means players fill the gaps by their imagination)

		DrawLights(cam, LightingRenderTexture, lights, AmbientLightColor);
		DrawLights(cam, VisionRenderTexture, visionLights, Color.Black);
	}

	public static void Dispose()
	{
		Clear();
		Raylib.UnloadRenderTexture(VisionRenderTexture);
		Raylib.UnloadRenderTexture(LightingRenderTexture);
	}

	public static void Clear()
	{
		foreach (var i in lights)
		{
			i.Dispose();
		}

		lights.Clear();
	}
}