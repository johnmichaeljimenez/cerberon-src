using System.Reflection;
using System.Runtime.Serialization;
using Main.Gameplay.Entities;
using Newtonsoft.Json;

namespace Main.Gameplay;

[Serializable]
public class World : IDisposable //aka Level loader
{
	//similar to Quake's worldspawn
	//World + other entities must be fully (at least 95%) serializable as JSON.
	//I will not make my own level editor, because I can make my own private Unity editor script and import/export the said JSON there.

	[JsonProperty]
	public List<BaseEntity> Entities { get; set; } = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toAddEntities = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toRemoveEntities = new();

	private static readonly Dictionary<string, Type> entityRegistry = new();

	private int _nextID;
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
		foreach (var i in Entities)
		{
			i.Init();
		}
	}

	public void Update(float dt)
	{
		foreach (var i in Entities)
		{
			if (i.IsDestroyed)
			{
				toRemoveEntities.Add(i);
				continue;
			}

			if (!i.IsActive)
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
		Raylib.DrawCircle(0, 0, 1, Color.Blue); //world origin sample

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

	public T SpawnEntity<T>() where T : BaseEntity
	{
		return SpawnEntity<T>(typeof(T).Name);
	}

	public T SpawnEntity<T>(string objectTypeName) where T : BaseEntity
	{
		if (!entityRegistry.TryGetValue(objectTypeName, out Type? type) || type == null || !typeof(T).IsAssignableFrom(type))
			throw new InvalidOperationException($"Unknown entity type: {objectTypeName}");

		object? objRaw = Activator.CreateInstance(type);
		if (objRaw is not T obj)
			throw new InvalidCastException($"Failed to create {objectTypeName} as {typeof(T).Name}");

		obj.ID = _nextID;
		_nextID++; //frequently spawned objects like bullets and particles will not be included in save serialization, and this ensures that IDs do not collide without slow checks

		obj.Init();

		toAddEntities.Add(obj);
		return obj;
	}

	public bool DespawnEntity(BaseEntity e)
	{
		return e.Despawn();
	}

	[OnDeserialized]
	protected void OnDeserialized(StreamingContext _)
	{
		_nextID = Entities.Count > 0 ? Entities.Max(e => e.ID) + 1 : 0;
	}
}