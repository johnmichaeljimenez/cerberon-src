namespace Cerberon.Helpers;

public class RefCount
{
	public bool IsActive { get; private set; }
	private readonly HashSet<string> activeIDs = new();

	public Action OnChanged;

	public bool Add(string id)
	{
		var result = activeIDs.Add(id);
		if (result)
			Update();

		return result;
	}

	public bool SetValue(string id, bool value)
	{
		if (value)
			return Add(id);

		return Remove(id);
	}

	public bool Remove(string id)
	{
		var result = activeIDs.Remove(id);
		if (result)
			Update();

		return result;
	}

	public bool Contains(string id)
	{
		return activeIDs.Contains(id);
	}

	public void Clear()
	{
		activeIDs.Clear();
		Update();
	}

	private void Update()
	{
		IsActive = activeIDs.Count > 0;
		OnChanged?.Invoke();
	}
}