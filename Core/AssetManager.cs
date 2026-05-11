using Main.Helpers;
using System.Text.RegularExpressions;

namespace Main.Core;

public class Sprite : IDisposable
{
	public const float PIXELS_PER_UNIT = 64;

	public string Name;
	public Texture2D Texture;
	public int Width { get; private set; }
	public int Height { get; private set; }
	public Vector2 UnitSize { get; private set; }
	public Rectangle Rect { get; private set; }

	public Sprite(string name, Texture2D texture2D)
	{
		Name = name;
		Texture = texture2D;
		Width = texture2D.Width;
		Height = texture2D.Height;
		UnitSize = new((float)Width / PIXELS_PER_UNIT, (float)Height / PIXELS_PER_UNIT);

		Rect = new(0, 0, Width, Height);
	}

	public void Dispose()
	{
		Raylib.UnloadTexture(Texture);
	}

	public void Draw9Sliced(Vector2 position, Vector2 size, float rotation, int sliceAmt = 36, Color? tint = null)
	{
		var tintColor = tint ?? Color.White;

		//let it operate on real pixel unit and let rlgl convert it to world space
		Vector2 sizePixels = size * PIXELS_PER_UNIT;

		Rlgl.PushMatrix();
		Rlgl.Translatef(position.X, position.Y, 0f);
		Rlgl.Scalef(1f / PIXELS_PER_UNIT, 1f / PIXELS_PER_UNIT, 1f);

		Raylib.DrawTextureNPatch(
			Texture,
			new NPatchInfo
			{
				Source = Rect,
				Layout = NPatchLayout.NinePatch,
				Left = sliceAmt,
				Right = sliceAmt,
				Top = sliceAmt,
				Bottom = sliceAmt
			},
			new Rectangle(0f, 0f, sizePixels.X, sizePixels.Y),
			sizePixels * 0.5f,
			rotation,
			tintColor
		);

		Rlgl.PopMatrix();
	}

	public void Draw(Vector2 position, float scale = 1, float rotation = 0, Color? tint = null, Vector2? origin = null, bool flipX = false, bool flipY = false)
	{
		var tintColor = tint ?? Color.White;
		var originNorm = origin ?? (Vector2.One * 0.5f);

		//render everything as fixed pixels per unit as I don't want to make art that has mismatched pixel density anyway even if the assets are not pixel art
		//i may be no artist, but I had exp in making pixel art and non-pixel art game assets and for me mismatched line art density is eyesore and amateur-level

		var destW = UnitSize.X * scale;
		var destH = UnitSize.Y * scale;

		var originPix = new Vector2(originNorm.X * destW, originNorm.Y * destH);
		var pivotPix = position;

		var destRect = new Rectangle(
			pivotPix.X,
			pivotPix.Y,
			destW,
			destH
		);

		var srcRect = new Rectangle(0, 0, flipX ? -Width : Width, flipY ? -Height : Height);

		Raylib.DrawTexturePro(Texture, srcRect, destRect, originPix, rotation, tintColor);
	}

	public void DrawTiled(Vector2 position, Vector2 tileSize, float rotation = 0, Color? tint = null)
	{
		var tintColor = tint ?? Color.White;
		var originNorm = Vector2.One * 0.5f;

		var destW = tileSize.X;
		var destH = tileSize.Y;

		var originPix = new Vector2(originNorm.X * destW, originNorm.Y * destH);
		var pivotPix = position;

		var destRect = new Rectangle(
			pivotPix.X,
			pivotPix.Y,
			destW,
			destH
		);

		var srcRect = new Rectangle(0, 0, Width, Height);
		Raylib.DrawTexturePro(Texture, srcRect, destRect, originPix, rotation, tintColor);
	}
}

public static class AssetManager
{
	private static readonly Dictionary<string, Sprite> sprites = new();
	private static readonly Dictionary<string, Animation> animations = new();
	public static readonly Dictionary<string, string> LevelFiles = new();
	public static Sprite MissingSprite { get; private set; }
	public static Font Font { get; private set; }

	//load everything in Assets for now regardless of where level they will be used. later I'll add an Update() function that stores the pending asset paths in a queue then timeslice them via Game's Update loop (true Raylib frames loop).
	//no multithreading bs as I need main thread to load textures, so I'll just do "load 10 png this frame then do the remaining 10 on next frame". good for loading screens too
	//my assets will (and should) not reach ~100mb anyway. and i believe that in the games that I will make, I will not exceed 300 sprites in a single camera view (even if identical/shared sprites).
	//no sprite atlas support as I don't need that and I am too lazy to make one (there's no real reliable way to make one nowadays that are engine-agnostic without manual work), but I know the REAL benefits of it from my work experience

	public static void Init()
	{
		var assetsPath = "Assets";
		Font = Raylib.LoadFont(Path.Combine(assetsPath, "font.ttf"));

		AudioHandler.Init(Path.Combine(assetsPath, "Audio"));

		var animationFile = Path.Combine(assetsPath, "animations.json");
		if (File.Exists(animationFile))
		{
			animations.Clear();
			var entries = File.ReadAllText(animationFile).FromJson<Dictionary<string, Animation>>();
			foreach (var i in entries)
			{
				animations.Add(i.Key, i.Value);
			}
		}

		var spritesPath = Path.Combine(assetsPath, "Sprites");

		if (!Directory.Exists(spritesPath))
		{
			Log.Send($"Warning: Assets folder not found at {spritesPath}");
			return;
		}

		var files = Directory.GetFiles(spritesPath, "*.png", SearchOption.AllDirectories);

		foreach (var file in files)
		{
			var relativePath = Path.GetRelativePath(spritesPath, file);
			var key = Path.ChangeExtension(relativePath, null).Replace('\\', '/');

			Texture2D tex = Raylib.LoadTexture(file);

			sprites[key] = new Sprite(key, tex);
			Console.WriteLine($"Loaded asset: {key}");
		}

		var chk = Raylib.GenImageChecked(128, 128, 4, 4, Color.Black, Color.Magenta);
		var chkTex = Raylib.LoadTextureFromImage(chk);
		MissingSprite = new Sprite("%missing%", chkTex);

		Raylib.UnloadImage(chk);

		foreach (var i in animations)
		{
			i.Value.Init();
		}

		LevelFiles.Clear();
		foreach (var i in Directory.GetFiles(Path.Combine(assetsPath, "Levels"), "*.json", SearchOption.AllDirectories))
		{
			LevelFiles.Add(i, Path.GetFileNameWithoutExtension(i));
		}
	}

	public static Animation GetAnimation(string name)
	{
		if (animations.TryGetValue(name, out var animation))
			return animation;

		return null; //TODO: add placeholder animation
	}

	public static Sprite GetSprite(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return null; //intentional null

		if (sprites.TryGetValue(name, out var sprite))
			return sprite;

		return MissingSprite;
	}

	public static void Dispose()
	{
		foreach (var i in sprites)
		{
			i.Value.Dispose();
		}

		MissingSprite.Dispose();
		sprites.Clear();

		AudioHandler.Unload();
		Raylib.UnloadFont(Font);
	}

	public static void OnDrawImGui()
	{
		var items = string.Join("\n", sprites.Values.Select(p => $"{p.Name}: {p.Width}x{p.Height}")); //super slow, but very temporary approach for now
		ImGui.Text($"Sprites: {sprites.Count}\n\n{items}");
	}

	public static List<string> GetSpritesStartingWith(string prefix)
	{
		if (string.IsNullOrEmpty(prefix))
			return new List<string>();

		return sprites.Keys
			.Where(k => k.StartsWith(prefix))
			.OrderBy(k => Regex.Replace(k, @"\d+", m => m.Value.PadLeft(10, '0')))
			.ToList();
	}
}