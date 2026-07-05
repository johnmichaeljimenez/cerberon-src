using Cerberon.Helpers;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Cerberon.Core;

public class Sprite : IDisposable
{
	public const float PIXELS_PER_UNIT = 64;

	public string Name;
	public Texture2D Texture;
	public int Width { get; private set; }
	public int Height { get; private set; }
	public Vector2 UnitSize { get; private set; }
	public Rectangle Rect { get; private set; }
	public SpriteMetadata Metadata { get; set; }

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

	public void Draw9Sliced(Vector2 position, Vector2 size, float rotation, int sliceAmt = 64, Color? tint = null)
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
		var originNorm = origin ?? Metadata.Origin;

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

public class SpriteMetadata
{
	public enum SpriteMaterial
	{
		None,
		Stone,
		Wood,
		Dirt,
		Concrete,
		Metal,
		Grass
	}

	public SpriteMaterial Material { get; set; } = SpriteMaterial.None;
	public bool StochasticTiling { get; set; }
	public Vector2 Origin { get; set; } = new(0.5f, 0.5f);
}

public class LoadRequest
{
	private const double TIME_BUDGET_MS = 16.0;
	private readonly Stopwatch stopwatch = new Stopwatch();
	private List<string> pending;
	private Action<string> onLoad;
	private Action onEnd;

	public bool Running { get; private set; }
	public int Count { get; private set; }
	public int Current { get; private set; }

	public LoadRequest(List<string> pending, Action<string> onLoad, Action onEnd)
	{
		this.pending = pending;
		this.onLoad = onLoad;
		this.onEnd = onEnd;

		Running = true;
		Count = pending.Count;
		Current = 0;
	}

	public void Update()
	{
		if (pending == null || pending.Count == 0) return;

		stopwatch.Restart();

		while (pending.Count > 0)
		{
			onLoad?.Invoke(pending[0]);
			pending.RemoveAt(0);
			Current++;

			if (pending.Count == 0)
			{
				onEnd?.Invoke();
				Running = false;
				stopwatch.Stop();
				break;
			}

			if (stopwatch.Elapsed.TotalMilliseconds >= TIME_BUDGET_MS)
			{
				stopwatch.Stop();
				break;
			}
		}
	}
}

public static class AssetManager
{
	private static readonly Dictionary<string, Sprite> sprites = new();
	private static readonly Dictionary<string, Animation> animations = new();
	private static readonly Dictionary<string, SpriteMetadata> spriteMetas = new();
	public static readonly Dictionary<string, string> LevelFiles = new();
	public static Sprite MissingSprite { get; private set; }
	public static Font Font { get; private set; }

	public static bool IsLoading { get; private set; }
	public static int CurrentLoadRequestCount { get; private set; }
	public static int MaxLoadRequestCount { get; private set; }
	public static float NormalizedRequestCount => (float)CurrentLoadRequestCount / MaxLoadRequestCount;
	private static readonly List<LoadRequest> loadRequests = new();
	private static Action onLoadEnd = null;

	//no sprite atlas support as I don't need that and I am too lazy to make one (there's no real reliable way to make one nowadays that are engine-agnostic without manual work), but I know the REAL benefits of it from my work experience
	public static void Init(Action onLoadEndAction = null)
	{
		IsLoading = true;
		onLoadEnd = onLoadEndAction;

		var assetsPath = "Assets";
		Font = Raylib.LoadFont(Path.Combine(assetsPath, "font.ttf"));
		ResetLoadRequest();

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

		var chk = Raylib.GenImageChecked(128, 128, 4, 4, Color.Black, Color.Magenta);
		var chkTex = Raylib.LoadTextureFromImage(chk);
		MissingSprite = new Sprite("%missing%", chkTex);
		Raylib.UnloadImage(chk);

		spriteMetas.Clear();
		var metaFile = Path.Combine(spritesPath, "meta.json");
		if (File.Exists(metaFile))
		{
			try
			{
				var metaJson = File.ReadAllText(metaFile);
				var settings = new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore,
					DefaultValueHandling = DefaultValueHandling.Populate,
					Converters = { new StringEnumConverter() }
				};
				var entries = JsonConvert.DeserializeObject<Dictionary<string, SpriteMetadata>>(metaJson, settings);
				if (entries != null)
				{
					foreach (var entry in entries)
					{
						spriteMetas[entry.Key] = entry.Value;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Send($"Error loading sprite meta.json at {metaFile}: {ex.Message}");
			}
		}

		var files = Directory.GetFiles(spritesPath, "*.png", SearchOption.AllDirectories).ToList();
		AddLoadRequest(files, file =>
		{
			var relativePath = Path.GetRelativePath(spritesPath, file);
			var key = Path.ChangeExtension(relativePath, null).Replace('\\', '/');

			Texture2D tex = Raylib.LoadTexture(file);

			var sprite = new Sprite(key, tex);
			if (spriteMetas.TryGetValue(key, out var meta))
			{
				sprite.Metadata = meta;
			}
			else
			{
				sprite.Metadata = new();
			}

			sprites[key] = sprite;
			Console.WriteLine($"Loaded asset: {key}");
		}, () =>
		{
			foreach (var i in animations)
			{
				i.Value.Init();
			}

			LevelFiles.Clear();
			foreach (var i in Directory.GetFiles(Path.Combine(assetsPath, "Levels"), "*.json", SearchOption.AllDirectories))
			{
				LevelFiles.Add(i, Path.GetFileNameWithoutExtension(i));
			}
		});
	}

	public static void AddLoadRequest(List<string> files, Action<string> onLoad, Action onEnd)
	{
		loadRequests.Add(new LoadRequest(files, onLoad, onEnd));
		MaxLoadRequestCount += files.Count;
	}

	public static void ResetLoadRequest()
	{
		loadRequests.Clear();
		MaxLoadRequestCount = 0;
		CurrentLoadRequestCount = 0;
	}

	//no multithreading allowed here
	public static void Update()
	{
		if (!IsLoading)
			return;

		var current = loadRequests[0];
		var prev = current.Current;
		current.Update();
		CurrentLoadRequestCount += current.Current - prev;

		if (!current.Running)
			loadRequests.RemoveAt(0);

		if (loadRequests.Count == 0)
		{
			IsLoading = false;
			ResetLoadRequest();
			onLoadEnd.Invoke();
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
		spriteMetas.Clear();

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