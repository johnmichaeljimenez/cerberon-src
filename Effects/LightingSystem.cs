using Cerberon.Core;
using Cerberon.Helpers;

namespace Cerberon.Effects;

public class Shadow
{
	public Vector2 Position;
	public Vector2 Size;
	public float Rotation;
	public bool Enabled = true;

	public Rectangle Bounds { get; private set; }

	public readonly Vector2[] Points;

	public Shadow(Vector2 centerPosition, Vector2 size, float rotation = 0f)
	{
		Position = centerPosition;
		Size = size;
		Rotation = rotation;

		Points = Utils.GetRectangleCorners(centerPosition, size, rotation).Reverse().ToArray();
		UpdateBounds();
	}

	public void Move(Vector2 delta)
	{
		Position += delta;
		for (int i = 0; i < Points.Length; i++)
		{
			Points[i] += delta;
		}

		UpdateBounds();
		LightingSystem.UpdateShadow(this);
	}

	private void UpdateBounds()
	{
		var sizeDouble = Size * 2;
		Bounds = new(Position - (sizeDouble * 0.5f), sizeDouble);
	}

	public void DrawShadow(Light light)
	{
		const float farDistance = 100;
		const float sideExtrude = 80;

		if (Points.Length == 4)
		{
			Raylib.DrawTriangle(Points[0], Points[1], Points[2], Color.Black);
			Raylib.DrawTriangle(Points[0], Points[2], Points[3], Color.Black);
		}

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

public class AmbientLight //cheap rectangular lights that doesn't cast shadows but overwrites global ambient light (can be used for indoor areas)
{
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	public float Rotation { get; set; }
	public Color Color { get; set; }
	public float AmbientMultiplier { get; set; }
	public bool Flicker { get; set; }

	public int FlickerSeed;

	public void Init()
	{
		var c = Color;
		var alphaRatio = c.A / 255.0f;

		var r = (byte)(c.R * alphaRatio);
		var g = (byte)(c.G * alphaRatio);
		var b = (byte)(c.B * alphaRatio);

		Color = new Color(r, g, b, (byte)255);

		if (Flicker)
			FlickerSeed = LightingSystem.GetFlickerSeed();
	}
}

public class Light : IDisposable
{
	public enum VisionEffects
	{
		Light,
		VisionOnly,
	}

	public enum ShadowTypes
	{
		None,
		Static, //one-shot generation
		Dynamic
	}

	public string SpriteID { get; set; }
	[JsonIgnore]
	private Sprite _sprite = null;
	[JsonIgnore]
	public Sprite Sprite
	{
		get
		{
			if (_sprite == null)
				_sprite = AssetManager.GetSprite(SpriteID);

			return _sprite;
		}
	}

	public Vector2 Position { get; set; }
	public Color Color { get; set; }
	public float Rotation { get; set; }
	public Vector2 Origin { get; set; }
	public float Scale { get; set; }
	public bool Enabled { get; set; }
	public VisionEffects VisionEffect { get; set; }
	public bool Flicker { get; set; }
	public string GroupID { get; set; } = "";
	public float AmbientMultiplier { get; set; } //0 = no ambient influence (indoor light), 1 = full ambient influence (outdoor light)

	public ShadowTypes ShadowType { get; set; }
	public RenderTexture2D? ShadowRenderTexture { get; private set; }
	public RenderTexture2D? GIRenderTexture { get; private set; }
	public Camera2D? ShadowCamera { get; private set; }
	public Rectangle Bounds { get; private set; }

	private const float SHADOW_MAP_RESOLUTION = 256f;
	private bool updatedShadow;

	public int FlickerSeed;

	public void ReupdateShadow()
	{
		updatedShadow = false;
	}

	public bool ShouldUpdateShadow()
	{
		if (ShadowType == ShadowTypes.Dynamic)
			return true;

		if (ShadowType == ShadowTypes.Static)
		{
			if (!updatedShadow)
			{
				updatedShadow = true;
				return true;
			}

			return false;
		}

		return false;
	}

	public Light(string spriteID, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null, ShadowTypes shadowType = Light.ShadowTypes.None, VisionEffects visionEffect = VisionEffects.Light)
	{
		var org = origin ?? new(0.5f, 0.5f);

		SpriteID = spriteID;
		Position = position;
		Color = color;
		Rotation = rotation;
		Origin = org;
		Scale = scale;
		Enabled = enabled;
		VisionEffect = visionEffect;
		ShadowType = shadowType;

		Init();
	}

	public void Init()
	{
		if (ShadowType != ShadowTypes.None)
		{
			int rtSize = (int)SHADOW_MAP_RESOLUTION; //TODO: use world unity dynamic resolution (bigger light = bigger RT)

			ShadowRenderTexture = Raylib.LoadRenderTexture(rtSize, rtSize);
			Raylib.SetTextureFilter(ShadowRenderTexture.Value.Texture, TextureFilter.Bilinear);

			int giSize = rtSize;
			GIRenderTexture = Raylib.LoadRenderTexture(giSize, giSize);
			Raylib.SetTextureFilter(GIRenderTexture.Value.Texture, TextureFilter.Bilinear);

			float spriteWorldDiameter = Sprite.UnitSize.X * Scale * 2;

			ShadowCamera = new Camera2D
			{
				Target = Position,
				Offset = new Vector2(rtSize / 2f, rtSize / 2f),
				Zoom = SHADOW_MAP_RESOLUTION / spriteWorldDiameter,
				Rotation = 0f
			};
		}

		var size = Sprite.UnitSize * Scale * 2;
		Bounds = new(Position - (size * 0.5f), size);
	}

	public void Dispose()
	{
		if (ShadowRenderTexture.HasValue)
			Raylib.UnloadRenderTexture(ShadowRenderTexture.Value);

		if (GIRenderTexture.HasValue)
			Raylib.UnloadRenderTexture(GIRenderTexture.Value);
	}
}

public static class LightingSystem
{
	public static RenderTexture2D LightingRenderTexture { get; private set; }
	public static RenderTexture2D VisionRenderTexture { get; private set; }
	public static Color AmbientLightColor { get; set; }
	public const float SCALE = 2.0f;

	private static readonly List<Light> lights = new();
	private static readonly List<AmbientLight> ambientLights = new();
	private static readonly List<Shadow> shadows = new();
	private static readonly List<Light> visionLights = new();
	private static readonly Dictionary<Shadow, List<Light>> nearbyStaticLights = new();

	private static readonly Dictionary<string, RefCount> lightGroups = new();

	private static Sprite ambientLightSprite;

	public static void Init(int width, int height)
	{
		ambientLightSprite = AssetManager.GetSprite("misc-softrect");

		LightingRenderTexture = Raylib.LoadRenderTexture((int)(width / SCALE), (int)(height / SCALE));
		Raylib.SetTextureFilter(LightingRenderTexture.Texture, TextureFilter.Bilinear);

		VisionRenderTexture = Raylib.LoadRenderTexture((int)(width / SCALE), (int)(height / SCALE));
		Raylib.SetTextureFilter(VisionRenderTexture.Texture, TextureFilter.Bilinear);

		Raylib.SetTextureFilter(AssetManager.GetSprite("light").Texture, TextureFilter.Bilinear);
		Raylib.SetTextureFilter(AssetManager.GetSprite("flashlight").Texture, TextureFilter.Bilinear);
	}

	public static void SetAmbientLights(IEnumerable<AmbientLight> list)
	{
		ambientLights.Clear();

		if (list == null)
			return;

		ambientLights.AddRange(list);
		ambientLights.ForEach(p => p.Init());
	}

	public static Light AddLight(Light light)
	{
		light.Init();

		if (light.VisionEffect == Light.VisionEffects.Light)
			lights.Add(light);
		else if (light.VisionEffect == Light.VisionEffects.VisionOnly)
			visionLights.Add(light);

		if (light.Flicker)
			light.FlickerSeed = GetFlickerSeed();

		if (string.IsNullOrEmpty(light.GroupID))
			light.GroupID = "<default>";

		if (!lightGroups.ContainsKey(light.GroupID))
			lightGroups[light.GroupID] = new();

		return light;
	}

	public static Light AddLight(string spriteID, Vector2 position, Color color, float rotation = 0f, float scale = 1, bool enabled = true, Vector2? origin = null, Light.ShadowTypes shadowType = Light.ShadowTypes.None, Light.VisionEffects visionEffect = Light.VisionEffects.Light)
	{
		var light = new Light(spriteID, position, color, rotation, scale, enabled, origin, shadowType, visionEffect);
		AddLight(light);

		return light;
	}

	public static void RemoveLight(Light light) //TODO (low prio) check all shadows containing this light and remove it, but no need for now
	{
		//no need to remove referenced light group entry here

		light.Dispose();

		if (lights.Contains(light))
			lights.Remove(light);

		if (visionLights.Contains(light))
			visionLights.Remove(light);
	}

	public static Shadow AddShadow(Vector2 centerPosition, Vector2 size, float rotation = 0f)
	{
		var shadow = new Shadow(centerPosition, size, rotation);
		shadows.Add(shadow);
		nearbyStaticLights[shadow] = new();

		foreach (var i in lights)
		{
			if (i.ShadowType != Light.ShadowTypes.Static || i.VisionEffect == Light.VisionEffects.VisionOnly)
				continue;

			var bounds = i.Bounds;
			if (Raylib.CheckCollisionRecs(shadow.Bounds, bounds))
				nearbyStaticLights[shadow].Add(i);
		}

		return shadow;
	}

	public static void RemoveShadow(Shadow shadow)
	{
		shadows.Remove(shadow);

		if (nearbyStaticLights.ContainsKey(shadow))
			nearbyStaticLights.Remove(shadow);
	}

	public static void SetLightGroupState(string id, bool disabled)
	{
		if (!lightGroups.ContainsKey(id))
			return;

		lightGroups[id].SetValue(id, disabled);
	}

	private static bool ShouldApplyGi(Light light)
	{
		return light.Enabled &&
			   light.ShadowType == Light.ShadowTypes.Static &&
			   light.VisionEffect == Light.VisionEffects.Light;
	}

	private static void DrawLights(Camera2D cam, RenderTexture2D tex, List<Light> l, Color bgColor, bool visionOnly)
	{
		foreach (var i in l)
		{
			if (!i.Enabled || (!visionOnly && lightGroups[i.GroupID].IsActive))
				continue;

			if (i.ShadowType == Light.ShadowTypes.None)
				continue;

			//nested render texture drawings are not allowed, so shadows first
			if (i.ShadowRenderTexture.HasValue && i.ShadowCamera.HasValue && i.ShouldUpdateShadow())
			{
				var rt = i.ShadowRenderTexture.Value;
				var lightCam = i.ShadowCamera.Value;

				lightCam.Target = i.Position;

				Raylib.BeginTextureMode(rt);
				Raylib.ClearBackground(Color.Black);
				Raylib.BeginMode2D(lightCam);

				i.Sprite.Draw(i.Position, tint: Color.White, rotation: i.Rotation, origin: i.Origin, scale: i.Scale); //let the actual light rendering handle color

				foreach (var shadow in shadows)
				{
					if (!shadow.Enabled)
						continue;

					shadow.DrawShadow(i);
				}

				Raylib.EndMode2D();
				Raylib.EndTextureMode();

				if (ShouldApplyGi(i) && i.GIRenderTexture.HasValue)
				{
					var giRt = i.GIRenderTexture.Value;
					Raylib.BeginTextureMode(giRt);
					Raylib.ClearBackground(Color.Black);

					Rectangle src = new Rectangle(0, 0, rt.Texture.Width, -rt.Texture.Height);
					Rectangle dest = new Rectangle(0, 0, giRt.Texture.Width, giRt.Texture.Height);

					var blur = RenderingManager.ShaderSets["Blur"];
					// blur.SetValue("resolution", new Vector2(giRt.Texture.Width, giRt.Texture.Height));
					// blur.SetValue("blurAmount", 6);
					Raylib.BeginShaderMode(blur.Shader);
					Raylib.DrawTexturePro(rt.Texture, src, dest, Vector2.Zero, 0f, Color.White);
					Raylib.EndShaderMode();


					Raylib.EndTextureMode();
				}
			}
		}

		Raylib.BeginTextureMode(tex);
		Raylib.BeginMode2D(cam);
		Raylib.ClearBackground(bgColor);
		bgColor.A = 255;

		if (!visionOnly)    //let ambient lights overwrite previous color as base light color before setting to additive
		{
			foreach (var i in ambientLights)
			{
				ambientLightSprite.Draw9Sliced(i.Position, i.Size, i.Rotation, tint: Color.Black);
			}

			foreach (var i in ambientLights)
			{
				var flicker = i.Flicker ? QuakeFlicker.GetIntensity(i.FlickerSeed) : 1.0f;
				if (flicker <= 0)
					continue;

				var color = Colors.Lerp(i.Color.Value(flicker), AmbientLightColor, i.AmbientMultiplier);
				color.A = 255;

				ambientLightSprite.Draw9Sliced(i.Position, i.Size, i.Rotation, tint: color);
			}
		}

		Raylib.BeginBlendMode(BlendMode.Additive);
		foreach (var i in l)
		{
			if (!i.Enabled || (!visionOnly && lightGroups[i.GroupID].IsActive))
				continue;

			var flicker = i.Flicker ? QuakeFlicker.GetIntensity(i.FlickerSeed) : 1.0f;
			if (flicker <= 0)
				continue;

			var color = Colors.Lerp(i.Color.Value(flicker), AmbientLightColor, i.AmbientMultiplier);

			if (i.ShadowType == Light.ShadowTypes.None)
			{
				i.Sprite.Draw(i.Position, tint: color, rotation: i.Rotation, origin: i.Origin, scale: i.Scale);
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

				// if (i.ShadowType == Light.ShadowTypes.Dynamic || visionOnly)
				Raylib.DrawTexturePro(rt.Texture, src, dest, Vector2.Zero, 0f, color);

				if (ShouldApplyGi(i) && i.GIRenderTexture.HasValue)
				{
					var giRt = i.GIRenderTexture.Value;
					Rectangle giSrc = new Rectangle(0, 0, giRt.Texture.Width, -giRt.Texture.Height);

					float giSizeMultiplier = 1.2f;
					float giWorldSize = worldSize * giSizeMultiplier;

					Rectangle giDest = new Rectangle(
						i.Position.X - giWorldSize / 2f,
						i.Position.Y - giWorldSize / 2f,
						giWorldSize,
						giWorldSize
					);

					Color giColor = new Color(
						(byte)(color.R * 0.5f),
						(byte)(color.G * 0.5f),
						(byte)(color.B * 0.5f),
						color.A
					);

					Raylib.DrawTexturePro(giRt.Texture, giSrc, giDest, Vector2.Zero, 0f, giColor);
				}
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

		DrawLights(cam, LightingRenderTexture, lights, AmbientLightColor, false);
		DrawLights(cam, VisionRenderTexture, visionLights, Color.Black, true);
	}

	public static void Dispose()
	{
		Clear();
		Raylib.UnloadRenderTexture(VisionRenderTexture);
		Raylib.UnloadRenderTexture(LightingRenderTexture);
	}

	public static void Clear()
	{
		nearbyStaticLights.Clear();

		foreach (var i in lights)
		{
			i.Dispose();
		}

		lightGroups.Clear();
		lights.Clear();
		ambientLights.Clear();
	}

	public static float GetOutdoorLightFactor(Vector2 position)
	{
		if (ambientLights.Count == 0)
			return 1;

		var n = 0;
		var f = 0f;
		foreach (var i in ambientLights)
		{
			if (!Utils.IsPointInRotatedRectangle(position, i.Position, i.Size, i.Rotation))
				continue;

			f += i.AmbientMultiplier;
			n++;
		}

		return n == 0 ? 1.0f : Raymath.Clamp01(f / n);
	}

	public static int GetFlickerSeed()
	{
		return RNG.Range(-5, 5);
	}

	public static void UpdateShadow(Shadow shadow)
	{
		foreach (var i in nearbyStaticLights[shadow])
		{
			i.ReupdateShadow();
		}
	}
}