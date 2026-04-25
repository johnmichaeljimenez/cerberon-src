using System.Collections.Concurrent;

namespace Main.Core;

public static class AssetWatcher
{
	private static readonly Dictionary<string, (FileSystemWatcher watcher, Action<string> callback)> _watchers = new();
	private static readonly Dictionary<string, bool> flags = new();

	public static string Add(string filePath, Action<string> onChanged)
	{
		var fullPath = Path.GetFullPath(filePath);
		var directory = Path.GetDirectoryName(fullPath) ?? ".";
		var fileName = Path.GetFileName(fullPath);

		var watcher = new FileSystemWatcher(directory, fileName)
		{
			NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
			EnableRaisingEvents = false
		};

		watcher.Changed += (sender, args) =>
		{
			flags[args.FullPath] = true;
		};

		watcher.EnableRaisingEvents = true;

		flags[fullPath] = false;
		_watchers[fullPath] = (watcher, onChanged);

		return File.ReadAllText(filePath);
	}

	public static void Remove(string filePath)
	{
		var fullPath = Path.GetFullPath(filePath);
		var entry = _watchers[fullPath];

		entry.watcher.EnableRaisingEvents = false;
		entry.watcher.Dispose();

		_watchers.Remove(fullPath);
		flags.Remove(fullPath);
	}

	public static void Update()
	{
		var keys = flags.Keys.ToList();

		foreach (var key in keys)
		{
			if (flags.TryGetValue(key, out bool changed) && changed)
			{
				if (TryReadAllTextSafe(key, out string content)) //just bruteforce it due to threading
				{
					flags[key] = false;
					_watchers[key].callback?.Invoke(content);
				}
			}
		}
	}

	private static bool TryReadAllTextSafe(string path, out string content)
	{
		content = "";

		try
		{
			using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new StreamReader(fs);
			content = reader.ReadToEnd();
			return true;
		}
		catch (IOException)
		{
			return false; //try again next Update()
		}
		catch (Exception ex)
		{
			Log.Send($"Error reading {path}: {ex.Message}");
			return false;
		}
	}

	public static void Dispose()
	{
		foreach (var kvp in _watchers)
		{
			kvp.Value.watcher.EnableRaisingEvents = false;
			kvp.Value.watcher.Dispose();
		}
		_watchers.Clear();
	}
}