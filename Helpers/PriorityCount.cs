namespace Main.Helpers;

public class PriorityCount
{
    public int Current { get; private set; }
    private readonly Dictionary<string, int> activeIDs = new();
    private readonly List<string> order = new(); // tracks insertion order, last = tail

    public Action<int> OnChanged;

    public void Add(string id, int value)
    {
        bool isNew = !activeIDs.ContainsKey(id);
        activeIDs[id] = value;

        if (isNew)
        {
            order.Add(id);
        }
        else
        {
            //for an update, move the id to the end to treat it as "last added"
            order.Remove(id);
            order.Add(id);
        }

        Update();
    }

    public void Remove(string id)
    {
        if (!activeIDs.ContainsKey(id))
            return;

        activeIDs.Remove(id);
        order.Remove(id);
        Update();
    }

    public void Clear()
    {
        activeIDs.Clear();
        order.Clear();
        Update();
    }

    private void Update()
    {
        if (order.Count > 0)
        {
            string lastId = order[order.Count - 1]; // tail
            Current = activeIDs[lastId];
        }
        else
        {
            Current = 0;
        }
        OnChanged?.Invoke(Current);
    }
}