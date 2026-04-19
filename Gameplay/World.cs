using System.Reflection;
using System.Runtime.Serialization;
using Main.Core;
using Main.Gameplay.Entities;
using Newtonsoft.Json;

namespace Main.Gameplay;

public struct WorldSettings //struct so that it cannot be null
{
	public Vector2 PlayerSpawnPoint;
}

[Serializable]
public class World : IDisposable //aka Level loader
{
	//similar to Quake's worldspawn
	//World + other entities must be fully (at least 95%) serializable as JSON.
	//I will not make my own level editor, because I can make my own private Unity editor script and import/export the said JSON there.

	public WorldSettings WorldSettings = new();

	[JsonProperty]
	public List<BaseEntity> Entities { get; private set; } = new();

	[JsonProperty]
	public List<CharacterEntity> CharacterEntities { get; private set; } = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toAddEntities = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toRemoveEntities = new();

	private static readonly Dictionary<string, Type> entityRegistry = new();

	[JsonIgnore]
	protected GameplayState gameplayState { get; private set; }

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

	public void Init(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
		_nextID = Entities.Count > 0 ? Entities.Max(e => e.ID) + 1 : 0;

		foreach (var i in Entities)
		{
			OnAdd(i);
			i.Init(gameplayState);
		}
	}

	public void Update(float dt, float udt)
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

			i.Update(dt, udt);
		}
	}

	public void LateUpdate(float dt, float udt)
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

			i.LateUpdate(dt, udt);
		}

		//do the finalization on the last step of update+late update loop of the world
		if (toAddEntities.Count > 0)
		{
			foreach (var i in toAddEntities)
			{
				Entities.Add(i);
				OnAdd(i);
			}

			toAddEntities.Clear();
		}

		if (toRemoveEntities.Count > 0)
		{
			foreach (var i in toRemoveEntities)
			{
				Entities.Remove(i);
				OnRemove(i);
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

			i.Draw(); //sorting is a tomorrow's problem
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

	public T SpawnEntity<T>(Action<T> onSpawn = null) where T : BaseEntity
	{
		return SpawnEntity<T>(typeof(T).Name, onSpawn);
	}

	public T SpawnEntity<T>(string objectTypeName, Action<T> onSpawn = null) where T : BaseEntity
	{
		if (!entityRegistry.TryGetValue(objectTypeName, out Type? type) || type == null || !typeof(T).IsAssignableFrom(type))
			throw new InvalidOperationException($"Unknown entity type: {objectTypeName}");

		object? objRaw = Activator.CreateInstance(type);
		if (objRaw is not T obj)
			throw new InvalidCastException($"Failed to create {objectTypeName} as {typeof(T).Name}");

		obj.ID = _nextID;
		_nextID++; //frequently spawned objects like bullets and particles will not be included in save serialization, and this ensures that IDs do not collide without slow checks

		onSpawn?.Invoke(obj); //set custom data here before Init() triggers
		obj.Init(gameplayState);

		toAddEntities.Add(obj);
		return obj;
	}

	public bool DespawnEntity(BaseEntity e)
	{
		return e.Despawn();
	}

	public void DrawDebug()
	{
		Raylib.DrawCircle(0, 0, 1, Colors.BLUE); //world origin sample

		foreach (var i in Entities)
		{
			if (i.IsDestroyed || !i.IsActive)
				continue;

			i.DrawDebug();
		}
	}

	public void DrawImGui()
	{
		ImGui.Text($"Entity count: {Entities.Count}");
		foreach (var i in Entities)
		{
			ImGui.Text($"[#{i.ID}] {i.GetType().Name}  |  {i.Position}");
		}
	}

	private void OnAdd(BaseEntity e)
	{
		if (e is CharacterEntity c)
			CharacterEntities.Add(c);
	}

	private void OnRemove(BaseEntity e)
	{
		if (e is CharacterEntity c)
			CharacterEntities.Remove(c);
	}
}