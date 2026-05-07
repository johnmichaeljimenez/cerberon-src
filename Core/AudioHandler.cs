using Main.Helpers;
using Tween;

namespace Main.Core;

public class MusicSource	//TODO: add float array to tell the Music track which are the nearest points to transition instead of doing it immediately on current time
{
	public Music Music;
	public string ID;
	public bool IsPlaying;
	public float Volume;
	public float LastTime;

	public void SetActive(bool isPlaying)
	{
		IsPlaying = isPlaying;
		Volume = isPlaying ? 0 : 1;
		TweenManager.Add(
			new Tween<float>(
				() => Volume,
				n => Volume = n,
				isPlaying ? 1 : 0, 0.4f,
				null,
				$"BGM-{ID}",
				true
			).SetEasing(Easing.Linear)
		);

		if (isPlaying)
		{
			Raylib.PlayMusicStream(Music);
			Raylib.SetMusicVolume(Music, Volume);

			if (LastTime > 0)
				Raylib.SeekMusicStream(Music, LastTime); //resume last music position if it already played
		}
	}
}

public class AudioSource
{
	public Sound Sound;
	public float Time;
	public bool IsSpatial;
	public Vector2 Position;
	public float Radius = 40;

	public void Update(Vector2 listener)
	{
		var d = listener - Position;
		var distance = d.Length();

		var volume = 1.0f - Raymath.Clamp01(distance / Radius); //linear for now
		if (volume <= 0.01f) volume = 0f;

		float pan = 0.5f;
		if (distance > 0.001f)
		{
			var dir = Raymath.Vector2Normalize(d);
			pan = 0.5f + dir.X * 0.5f;
		}

		Raylib.SetSoundVolume(Sound, volume);
		Raylib.SetSoundPan(Sound, pan); //0 = full left, 0.5 = center, 1 = full right
	}
}

public static class AudioHandler
{
	private const int ALIAS_COUNT = 10;
	private const float MUSIC_BASE_VOLUME = 0.4f;

	private static readonly List<AudioSource> activeAudioSources = new();
	private static readonly Dictionary<string, MusicSource> musicAssets = new();
	private static readonly Dictionary<string, float> soundLengths = new();
	private static readonly Dictionary<string, Sound> soundAssets = new();
	private static readonly Dictionary<string, List<Sound>> soundAliases = new();
	private static readonly Dictionary<string, int> nextAliasIndex = new();
	private static readonly Dictionary<string, List<string>> soundVariations = new();

	public static Vector2 ListenerPosition { get; set; }
	public static MusicSource CurrentMusic { get; set; }

	//soundAssets -> soundAliases -> soundVariations

	public static void Init(string path)
	{
		Raylib.InitAudioDevice();
		LoadAllSounds(path);
	}

	public static void Update()
	{
		foreach (var music in musicAssets.Values)
		{
			if (IsMusicPlaying(music.Music))
			{
				Raylib.UpdateMusicStream(music.Music);
				Raylib.SetMusicVolume(music.Music, music.Volume * MUSIC_BASE_VOLUME * (PauseHandler.IsPaused? 0.4f : 1));

				if (!music.IsPlaying && music.Volume <= 0.01f) //requested to stop and volume is near zero
				{
					music.LastTime = Raylib.GetMusicTimePlayed(music.Music);
					Raylib.StopMusicStream(music.Music);
				}
			}
			else if (music.IsPlaying)
			{
				Raylib.PlayMusicStream(music.Music); //loop
			}
		}

		for (int i = activeAudioSources.Count - 1; i >= 0; i--)
		{
			var a = activeAudioSources[i];
			if (Utils.Countdown(ref a.Time, Time.DeltaTime))
			{
				activeAudioSources.RemoveAt(i);
				continue;
			}

			if (!a.IsSpatial)
				continue;

			a.Update(ListenerPosition);
		}
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

		foreach (var music in musicAssets.Values)
		{
			Raylib.UnloadMusicStream(music.Music);
		}

		musicAssets.Clear();
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

			if (!string.IsNullOrEmpty(groupKey) && groupKey.StartsWith("bgm", StringComparison.OrdinalIgnoreCase)) //all audios under Audio/bgm is treated as music automatically
			{
				foreach (var filePath in files)
				{
					string fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
					string musicKey = $"{groupKey}";

					Music music = Raylib.LoadMusicStream(filePath);
					musicAssets[musicKey] = new()
					{
						ID = musicKey,
						IsPlaying = false,
						Volume = 0,
						Music = music
					};
					totalLoaded++;
				}
				continue;
			}

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

				soundLengths.Add(individualKey, ((float)source.FrameCount / (float)source.Stream.SampleRate) + 0.5f); //padding
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

	public static Sound? PlaySound(string key, Vector2? position = null)
	{
		if (soundVariations.TryGetValue(key, out var variations) && variations.Count > 0)
		{
			string chosen = variations[Random.Shared.Next(variations.Count)];
			return PlayIndividual(chosen, position);
		}

		return PlayIndividual(key, position);
	}

	private static Sound? PlayIndividual(string individualKey, Vector2? position)
	{
		if (!soundAliases.TryGetValue(individualKey, out var aliases) || aliases.Count == 0)
			return null;

		var index = nextAliasIndex.GetValueOrDefault(individualKey, 0);
		var sound = aliases[index];

		Raylib.PlaySound(aliases[index]);
		Raylib.SetSoundPan(sound, 0.5f);
		Raylib.SetSoundVolume(sound, 1.0f);

		var source = new AudioSource()
		{
			Sound = sound,
			Time = soundLengths[individualKey],
			IsSpatial = position.HasValue,
			Position = position ?? Vector2.Zero
		};

		if (source.IsSpatial)
			source.Update(ListenerPosition);

		activeAudioSources.Add(source);
		Raylib.SetSoundPitch(sound, RNG.Range(0.9f, 1.1f));

		nextAliasIndex[individualKey] = (index + 1) % aliases.Count;
		return sound;
	}

	public static bool IsPlaying(Sound sound)
	{
		return Raylib.IsSoundPlaying(sound);
	}

	public static MusicSource PlayMusic(string key)
	{
		if (!musicAssets.TryGetValue(key, out var music))
			return null;

		if (CurrentMusic == music && music.IsPlaying)
			return music;

		StopMusic();

		CurrentMusic = music;
		CurrentMusic.SetActive(true);
		return music;
	}

	public static void StopMusic()
	{
		if (CurrentMusic == null)
			return;

		CurrentMusic.SetActive(false);
		CurrentMusic = null;
	}

	public static void ClearMusicStates()
	{
		foreach (var i in musicAssets)
		{
			i.Value.IsPlaying = false;
			i.Value.Volume = 0;
			i.Value.LastTime = 0;

			if (Raylib.IsMusicStreamPlaying(i.Value.Music))
				Raylib.StopMusicStream(i.Value.Music);
		}
	}

	public static bool IsMusicPlaying(Music music)
	{
		return Raylib.IsMusicStreamPlaying(music);
	}
}