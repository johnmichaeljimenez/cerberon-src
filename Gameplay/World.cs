using System.Reflection;
using System.Runtime.Serialization;
using Main.Core;
using Main.Effects;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;
using Main.Helpers;
using Newtonsoft.Json;

namespace Main.Gameplay;

[Serializable]
public class WorldSpriteRenderer //no need for real entities for static environment stuff like these
{
	public enum RenderTypes
	{
		Default,
		Tiled,
		Masked
	}

	[JsonProperty]
	public string SpriteID { get; set; }

	public Sprite Sprite { get; set; }
	public Vector2 Position { get; set; }
	public float Rotation { get; set; }
	public float Scale { get; set; } = 1;
	public Color Tint { get; set; } = Color.White;
	public int SortingGroup { get; set; }
	public int SortingIndex { get; set; }
	public float Parallax { get; set; }
	public RenderTypes RenderType { get; set; }
	public Vector2 TileSize { get; set; }

	public void Init()
	{
		Sprite = AssetManager.GetSprite(SpriteID);
	}

	public void Draw()
	{
		var pos = Position;
		if (MathF.Abs(Parallax) >= 0.001f)
		{
			var dir = Position - Game.Instance.Camera.Camera.Target;
			pos += dir * Parallax; // value > 0 for tall objects, < 0 for low objects
		}

		if (RenderType == RenderTypes.Tiled)
		{
			RenderingManager.BeginTiledShader(Sprite, TileSize);
			Sprite.DrawTiled(pos, TileSize, Rotation, Tint);
			Raylib.EndShaderMode();
		}
		else
		{
			Sprite.Draw(pos, Scale, Rotation, Tint, Vector2.One * 0.5f);
		}
	}
}


public struct WorldSettings //struct so that it cannot be null
{
	public Vector2 PlayerSpawnPoint;
	public Color AmbientColor;
	public Vector2 WorldSize; //intentionally closed-space world
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
	public List<WorldSpriteRenderer> EnvironmentSprites { get; private set; } = new();

	[JsonIgnore]
	public readonly Dictionary<int, List<WorldSpriteRenderer>> Sprites = new();

	[JsonIgnore]
	public readonly Dictionary<string, List<BaseEntity>> EntityGroups = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toAddEntities = new();

	[JsonIgnore]
	private readonly List<BaseEntity> toRemoveEntities = new();

	private static readonly Dictionary<string, Type> entityRegistry = new();

	[JsonIgnore]
	protected GameplayState gameplayState { get; private set; }

	private readonly List<Wall> worldBounds = new();

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

		gameplayState.GetManager<WaypointManager>().Bake(Entities.Where(p => p is WallEntity).Cast<WallEntity>().Select(p => p.RectangleBounds), WorldSettings.WorldSize, 1.5f); //TODO: add "is solid" property for entities once static props are implemented

		foreach (var i in Entities)
		{
			OnAdd(i);
			i.Init(gameplayState);
		}

		gameplayState.GetManager<CollisionManager>().AddWalls(-WorldSettings.WorldSize * 0.5f, WorldSettings.WorldSize, worldBounds, true);
		LightingSystem.AmbientLightColor = WorldSettings.AmbientColor;

		DecalSystem.Init(Vector2.Zero, WorldSettings.WorldSize);

		Sprites.Clear();
		foreach (var i in EnvironmentSprites)
		{
			i.Init();

			if (!Sprites.ContainsKey(i.SortingGroup))
				Sprites[i.SortingGroup] = new();

			Sprites[i.SortingGroup].Add(i);
		}

		foreach (var kvp in Sprites)
		{
			kvp.Value.Sort((a, b) => a.SortingIndex.CompareTo(b.SortingIndex));
		}

		if (!Sprites.ContainsKey(0))
			Sprites.Add(0, new()); //add middleground rendering for entity and decal's baseline layer
	}

	public void Update(float dt, float udt)
	{
		if (PauseHandler.IsPaused)
			return;

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
		foreach (var i in Sprites)
		{
			foreach (var j in i.Value)
			{
				j.Draw();
			}

			if (i.Key == 0)
			{
				DecalSystem.Draw();

				foreach (var j in Entities)
				{
					if (j.IsDestroyed || !j.IsActive)
						continue;

					j.Draw(); //sorting for each entities is a tomorrow's problem (or maybe this is already fine now)
				}
			}
		}
	}

	public void Dispose()
	{
		foreach (var i in worldBounds)
		{
			gameplayState.GetManager<CollisionManager>().RemoveWall(i);
		}

		DecalSystem.Dispose();
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
		return SpawnEntity(typeof(T).Name, onSpawn);
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

		foreach (var i in worldBounds)
		{
			Utils.DrawLineEx(i.From, i.To, i.Midpoint, i.Normal, Colors.RED);
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
		foreach (var i in e.Groups)
		{
			if (!EntityGroups.ContainsKey(i))
				EntityGroups[i] = new();

			EntityGroups[i].Add(e);
		}
	}

	private void OnRemove(BaseEntity e)
	{
		foreach (var i in e.Groups)
		{
			if (EntityGroups.ContainsKey(i) && EntityGroups[i].Contains(e))
				EntityGroups[i].Remove(e);
		}
	}

	public List<BaseEntity> GetEntitiesByGroup(string groupName)
	{
		if (!EntityGroups.ContainsKey(groupName))
			return new(); //TODO: optimize

		return EntityGroups[groupName];
	}
}