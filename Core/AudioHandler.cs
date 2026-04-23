namespace Main.Core;

public static class AudioHandler
{
	private const int ALIAS_COUNT = 10;

	private static readonly Dictionary<string, Sound> soundAssets = new();
	private static readonly Dictionary<string, List<Sound>> soundAliases = new();
	private static readonly Dictionary<string, int> nextAliasIndex = new();
	private static readonly Dictionary<string, List<string>> soundVariations = new();

	//soundAssets -> soundAliases -> soundVariations

	public static void Init(string path)
	{
		Raylib.InitAudioDevice();
		LoadAllSounds(path);
	}

	public static void Unload()
	{
		foreach (var aliases in soundAliases.Values)
		{
			foreach (var alias in aliases)
			{
				Raylib.UnloadSoundAlias(alias);
			}
		}

		foreach (var sound in soundAssets.Values)
		{
			Raylib.UnloadSound(sound);
		}

		soundAssets.Clear();
		soundAliases.Clear();
		nextAliasIndex.Clear();
		soundVariations.Clear();

		Raylib.CloseAudioDevice();
	}

	public static void LoadAllSounds(string rootPath)
	{
		if (!Directory.Exists(rootPath))
		{
			return;
		}

		var audioExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".wav", ".ogg", ".mp3", ".flac"
		};

		var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories)
			.Where(f => audioExtensions.Contains(Path.GetExtension(f)))
			.ToList();

		var fileGroups = allFiles
			.GroupBy(f => Path.GetDirectoryName(f) ?? "")
			.ToDictionary(
				g => GetRelativeFolderKey(rootPath, g.Key),
				g => g.OrderBy(f => f).ToList());

		int totalLoaded = 0;

		foreach (var kvp in fileGroups)
		{
			string groupKey = kvp.Key;
			var files = kvp.Value;

			if (files.Count == 0) continue;

			if (string.IsNullOrEmpty(groupKey))
			{
				foreach (var filePath in files)
				{
					string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
					string individualKey = fileNameNoExt;

					Sound source = Raylib.LoadSound(filePath);
					RegisterSound(individualKey, source);

					soundVariations[individualKey] = new List<string> { individualKey };
					totalLoaded++;
				}
				continue;
			}

			var variationKeys = new List<string>(files.Count);
			foreach (var filePath in files)
			{
				string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
				string individualKey = $"{groupKey}/{fileNameNoExt}";

				Sound source = Raylib.LoadSound(filePath);
				RegisterSound(individualKey, source);
				variationKeys.Add(individualKey);
			}

			soundVariations[groupKey] = variationKeys;
			totalLoaded += variationKeys.Count;
		}
	}

	private static string GetRelativeFolderKey(string rootPath, string? fullDir)
	{
		if (string.IsNullOrEmpty(fullDir)) return "";
		string relative = Path.GetRelativePath(rootPath, fullDir);
		if (relative == ".") return "";
		return relative.Replace('\\', '/');
	}

	public static void LoadSoundGroup(string groupKey, string? folderPath = null)
	{
		folderPath ??= groupKey;
		if (!Directory.Exists(folderPath))
		{
			return;
		}

		var extensions = new[] { "*.wav", "*.ogg", "*.mp3", "*.flac", "*.aiff" };
		var files = extensions
			.SelectMany(ext => Directory.GetFiles(folderPath, ext))
			.OrderBy(f => f)
			.ToList();

		var variationKeys = new List<string>();
		foreach (var file in files)
		{
			string fileName = Path.GetFileNameWithoutExtension(file);
			string individualKey = $"{groupKey}/{fileName}";

			Sound source = Raylib.LoadSound(file);
			RegisterSound(individualKey, source);
			variationKeys.Add(individualKey);
		}

		if (variationKeys.Count > 0)
			soundVariations[groupKey] = variationKeys;
	}

	private static void RegisterSound(string key, Sound source)
	{
		soundAssets[key] = source;

		var aliases = new List<Sound>(ALIAS_COUNT);
		for (int i = 0; i < ALIAS_COUNT; i++)
			aliases.Add(Raylib.LoadSoundAlias(source));

		soundAliases[key] = aliases;
		nextAliasIndex[key] = 0;
	}

	public static bool PlaySound(string key)
	{
		if (soundVariations.TryGetValue(key, out var variations) && variations.Count > 0)
		{
			string chosen = variations[Random.Shared.Next(variations.Count)];
			return PlayIndividual(chosen);
		}

		return PlayIndividual(key);
	}

	private static bool PlayIndividual(string individualKey)
	{
		if (!soundAliases.TryGetValue(individualKey, out var aliases) || aliases.Count == 0)
			return false;

		int index = nextAliasIndex.GetValueOrDefault(individualKey, 0);
		Raylib.PlaySound(aliases[index]);
		nextAliasIndex[individualKey] = (index + 1) % aliases.Count;
		return true;
	}
}