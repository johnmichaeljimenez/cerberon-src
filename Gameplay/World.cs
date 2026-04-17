using System.Reflection;
using Newtonsoft.Json;

namespace Main.Gameplay;

[Serializable]
public class World : IDisposable //aka Level loader
{
	//similar to Quake's worldspawn
	//World + other entities must be fully (at least 95%) serializable as JSON.
	//I will not make my own level editor, because I can make my own private Unity editor script and import/export the said JSON there.

	//but right now I am lost on how to make my entire world serializable but at the same time, they have custom data from level editor.
	//for example, I have a "breakable object" entity. then I want that to serialize where it can save its position, current life, etc. in current save file JSON but it also has data defined by level design JSON like what sprite it is, max life, etc.

	[JsonProperty]
	public List<BaseEntity> Entities { get; set; } = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toAddEntities = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toRemoveEntities = new();

	private static readonly Dictionary<string, Type> entityRegistry = new();
	public static void InitRegistry()
	{
		entityRegistry.Clear();
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes()
		.Where(t => t.IsClass && !t.IsAbstract && typeof(BaseEntity).IsAssignableFrom(t)))
		{
			entityRegistry[type.Name] = type;
		}
	}

	public void Init()
	{

	}

	public void Update(float dt)
	{
		foreach (var i in Entities)
		{
			if (i.IsDestroyed || !i.IsActive)
				continue;

			i.Update(dt);
		}

		if (toAddEntities.Count > 0)
		{
			foreach (var i in toAddEntities)
			{
				Entities.Add(i);
			}

			toAddEntities.Clear();
		}

		if (toRemoveEntities.Count > 0)
		{
			foreach (var i in toRemoveEntities)
			{
				Entities.Remove(i);
			}

			toRemoveEntities.Clear();
		}
	}

	public void Draw()
	{
		foreach (var i in Entities)
		{
			if (i.IsDestroyed || !i.IsActive)
				continue;

			i.Draw();
		}
	}

	public void Dispose()
	{
		DisposeAllEntities();
	}

	private void DisposeAllEntities()
	{
		foreach (var entity in Entities) entity.Dispose();
		foreach (var entity in toAddEntities) entity.Dispose();
		foreach (var entity in toRemoveEntities) entity.Dispose();
		Entities.Clear();
		toAddEntities.Clear();
		toRemoveEntities.Clear();
	}

	public T SpawnEntity<T>(string objectTypeName) where T : BaseEntity
	{
		if (!entityRegistry.TryGetValue(objectTypeName, out Type? type) || type == null || !typeof(T).IsAssignableFrom(type))
			throw new InvalidOperationException($"Unknown entity type: {objectTypeName}");

		object? objRaw = Activator.CreateInstance(type);
		if (objRaw is not T obj)
			throw new InvalidCastException($"Failed to create {objectTypeName} as {typeof(T).Name}");

		obj.ID = GetNextID();

		toAddEntities.Add(obj);
		return obj;
	}

	private int GetNextID()
	{
		int maxID = 0;
		if (Entities.Count > 0) maxID = Entities.Max(e => e.ID);
		if (toAddEntities.Count > 0) maxID = Math.Max(maxID, toAddEntities.Max(e => e.ID));

		return maxID + 1;
	}

	public bool DespawnEntity(BaseEntity e)
	{
		var d = e.Despawn();
		if (d)
			toRemoveEntities.Add(e);

		return d;
	}
}