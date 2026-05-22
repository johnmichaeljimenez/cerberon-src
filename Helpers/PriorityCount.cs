namespace Main.Helpers;

public class PriorityCount
{
	public int Current { get; private set; }
	private readonly Dictionary<string, int> activeIDs = new();

	public Action<int> OnChanged;

	public void Add(string id, int value)
	{
		activeIDs[id] = value;	//allow updating if it exists
		Update();
	}

	public void Remove(string id)
	{
		if (!activeIDs.ContainsKey(id))
			return;

		activeIDs.Remove(id);
		Update();
	}

	public void Clear()
	{
		activeIDs.Clear();
		Update();
	}

	private void Update()
	{
		Current = activeIDs.Count > 0? activeIDs.Max(p => p.Value) : 0;	//TODO: optimize if needed
		OnChanged?.Invoke(Current);
	}
}