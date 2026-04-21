namespace Main.Core;

public class Sprite : IDisposable
{
	public const float PIXELS_PER_UNIT = 64;

	public string Name;
	public Texture2D Texture;
	public int Width, Height;

	public void Dispose()
	{
		Raylib.UnloadTexture(Texture);
	}

	public void Draw(Vector2 position, float scale = 1, float rotation = 0, Color? tint = null, Vector2? origin = null, bool flipX = false, bool flipY = false)
	{
		var tintColor = tint ?? Color.White;
		var originNorm = origin ?? (Vector2.One * 0.5f);

		//render everything as fixed pixels per unit as I don't want to make art that has mismatched pixel density anyway even if the assets are not pixel art
		//i may be no artist, but I had exp in making pixel art and non-pixel art game assets and for me mismatched line art density is eyesore and amateur-level
		var destW = ((float)Width / PIXELS_PER_UNIT) * scale; //TODO: precalc on sprite load
		var destH = ((float)Height / PIXELS_PER_UNIT) * scale;

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
}

public static class AssetManager
{
	private static readonly Dictionary<string, Sprite> sprites = new();
	private static readonly Dictionary<string, Animation> animations = new();
	public static Sprite MissingSprite { get; private set; }

	//load everything in Assets for now regardless of where level they will be used. later I'll add an Update() function that stores the pending asset paths in a queue then timeslice them via Game's Update loop (true Raylib frames loop).
	//no multithreading bs as I need main thread to load textures, so I'll just do "load 10 png this frame then do the remaining 10 on next frame". good for loading screens too
	//my assets will (and should) not reach ~100mb anyway. and i believe that in the games that I will make, I will not exceed 300 sprites in a single camera view (even if identical/shared sprites).
	//no sprite atlas support as I don't need that and I am too lazy to make one (there's no real reliable way to make one nowadays that are engine-agnostic without manual work), but I know the REAL benefits of it from my work experience

	public static void Init()
	{
		string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets/Sprites");

		if (!Directory.Exists(assetsPath))
		{
			Console.WriteLine($"Warning: Assets folder not found at {assetsPath}");
			return;
		}

		string[] files = Directory.GetFiles(assetsPath, "*.png", SearchOption.AllDirectories);

		foreach (string file in files)
		{
			string relativePath = Path.GetRelativePath(assetsPath, file);
			string key = Path.ChangeExtension(relativePath, null).Replace('\\', '/');

			Texture2D tex = Raylib.LoadTexture(file);

			sprites[key] = new Sprite
			{
				Name = key,
				Texture = tex,
				Width = tex.Width,
				Height = tex.Height
			};

			Console.WriteLine($"Loaded asset: {key}");
		}

		var chk = Raylib.GenImageChecked(128, 128, 4, 4, Color.Black, Color.Magenta);
		var chkTex = Raylib.LoadTextureFromImage(chk);
		MissingSprite = new Sprite()
		{
			Name = "%missing%",
			Width = chkTex.Width,
			Height = chkTex.Height,
			Texture = chkTex
		};

		Raylib.UnloadImage(chk);
	}

	public static Animation GetAnimation(string name)
	{
		if (animations.TryGetValue(name, out var animation))
			return animation;

		return null; //TODO: add placeholder animation
	}

	public static Sprite GetSprite(string name)
	{
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
	}

	public static void OnDrawImGui()
	{
		var items = string.Join("\n", sprites.Values.Select(p => $"{p.Name}: {p.Width}x{p.Height}")); //super slow, but very temporary approach for now
		ImGui.Text($"Sprites: {sprites.Count}\n\n{items}");
	}
}