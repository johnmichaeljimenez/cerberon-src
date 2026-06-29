using System.Collections.Concurrent;

namespace Cerberon.Core;

public static class AssetWatcher
{
	private static readonly ConcurrentDictionary<string, (FileSystemWatcher watcher, Action<string> callback)> _watchers = new();
	private static readonly ConcurrentDictionary<string, bool> flags = new();

	public static string Add(string filePath, Action<string> onChanged)
	{
		var fullPath = Path.GetFullPath(filePath);
		var directory = Path.GetDirectoryName(fullPath) ?? ".";
		var fileName = Path.GetFileName(fullPath);

		if (_watchers.TryRemove(fullPath, out var oldEntry))
		{
			oldEntry.watcher.EnableRaisingEvents = false;
			oldEntry.watcher.Dispose();
			flags.TryRemove(fullPath, out _);
		}

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

		if (_watchers.TryRemove(fullPath, out var entry))
		{
			entry.watcher.EnableRaisingEvents = false;
			entry.watcher.Dispose();
		}

		flags.TryRemove(fullPath, out _);
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
					flags[key] = false;           // mark as processed
					if (_watchers.TryGetValue(key, out var entry))
					{
						entry.callback?.Invoke(content);
					}
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
		// snapshot to avoid issues if events fire during disposal
		foreach (var kvp in _watchers.ToArray())
		{
			kvp.Value.watcher.EnableRaisingEvents = false;
			kvp.Value.watcher.Dispose();
		}
		_watchers.Clear();
		flags.Clear();
	}
}