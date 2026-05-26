using System.Reflection;
using Main.Core;
using Main.Effects;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Level;

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
	public List<Trigger> Triggers { get; private set; } = new();

	[JsonProperty]
	public List<Light> Lights { get; private set; } = new();

	[JsonProperty]
	public List<AmbientLight> AmbientLights { get; private set; } = new();

	[JsonProperty]
	public List<WorldCollider> EnvironmentColliders { get; private set; } = new();

	[JsonProperty]
	public List<WorldSpriteRenderer> EnvironmentSprites { get; private set; } = new();

	[JsonProperty]
	public List<WorldMarker> Markers { get; private set; } = new();

	[JsonIgnore]
	public readonly SortedDictionary<int, List<WorldSpriteRenderer>> Sprites = new(); //ensure that key is sorted (ascending)

	[JsonIgnore]
	public readonly Dictionary<string, List<BaseEntity>> EntityGroups = new();

	private readonly HashSet<BaseEntity> toAddEntities = new(); //avoid duplicate spawning/despawning request
	private readonly HashSet<BaseEntity> toRemoveEntities = new();

	private static readonly Dictionary<string, Type> entityRegistry = new();

	[JsonIgnore]
	protected GameplayState gameplayState { get; private set; }

	private readonly List<Wall> worldBounds = new();

	private int _nextID;

	public readonly Signal<BaseEntity> OnEntityDespawn = new();
	private readonly Dictionary<WorldCollider, (List<Wall>, Shadow)> colliderWalls = new();

	private Sprite wallSprite;

	[JsonIgnore]
	public NodeData NodeData { get; private set; }

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

		if (Markers == null)
			Markers = new();

		if (EnvironmentSprites == null)
			EnvironmentSprites = new();

		if (Lights == null)
			Lights = new();

		if (AmbientLights == null)
			AmbientLights = new();

		wallSprite = AssetManager.GetSprite("misc-softrect");

		foreach (var i in EnvironmentColliders)
		{
			var l = new List<Wall>();
			Shadow shadow = null;

			gameplayState.GetManager<CollisionManager>().AddWalls(i.Position, i.Size, l, i.Flags, i.Height, false, i.Rotation);

			if (i.Height >= CollisionHeight.High)
				shadow = LightingSystem.AddShadow(i.Position, i.Size, i.Rotation);

			colliderWalls.Add(i, (l, shadow));
		}

		foreach (var i in Entities)
		{
			i.Init(gameplayState);
			OnAdd(i);
			i.ModulesInit();
		}

		gameplayState.GetManager<WaypointManager>().Bake(EnvironmentColliders, WorldSettings.WorldSize, 1f, Entities.Where(p => p is IWaypointModifier).Cast<IWaypointModifier>().ToList());
		gameplayState.GetManager<CollisionManager>().AddWalls(Vector2.Zero, WorldSettings.WorldSize, worldBounds, Wall.WallFlags.None, CollisionHeight.Low, true);

		LightingSystem.AmbientLightColor = WorldSettings.AmbientColor;
		foreach (var i in Lights)
		{
			LightingSystem.AddLight(i);
		}

		LightingSystem.SetAmbientLights(AmbientLights); //ambient lights are static anyway

		DecalSystem.Init(Vector2.Zero, WorldSettings.WorldSize);

		Sprites.Clear();

		foreach (var i in EnvironmentSprites)
		{
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

		gameplayState.GetManager<TriggerManager>().SetupTriggers(Triggers);
		NodeData = new(gameplayState);
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
			i.ModulesUpdate(dt, udt);
		}
	}

	public void LateUpdate(float dt, float udt)
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

			i.LateUpdate(dt, udt);
			i.ModulesLateUpdate(dt, udt);
		}

		var changed = false;
		//do the finalization on the last step of update+late update loop of the world
		if (toAddEntities.Count > 0)
		{
			changed = true;
			foreach (var i in toAddEntities)
			{
				Entities.Add(i);
				OnAdd(i);
			}

			toAddEntities.Clear();
		}

		if (toRemoveEntities.Count > 0)
		{
			changed = true;
			foreach (var i in toRemoveEntities)
			{
				i.Dispose();
				OnEntityDespawn.Publish(i);
				Entities.Remove(i);
				OnRemove(i);
			}

			toRemoveEntities.Clear();
		}

		if (changed)
		{
			Entities.Sort((p, q) => p.SortingIndex.CompareTo(q.SortingIndex)); //ascending
		}
	}

	public void Draw()
	{
		foreach (var i in Sprites)
		{
			if (i.Key == 0)
			{
				DecalSystem.Draw();
			}

			foreach (var j in i.Value)
			{
				j.Draw();
			}

			if (i.Key == 0)
			{
				RenderingManager.BeginEntityShader();
				var shadowSprite = AssetManager.GetSprite("blob-shadow");
				foreach (var j in Entities)
				{
					if (j.IsDestroyed || !j.IsActive)
						continue;

					if (j is not CharacterEntity c)
						continue;

					shadowSprite.Draw(j.Position, c.CollisionBody.Radius * 4, 0, Color.Black);
				}

				foreach (var j in Entities)
				{
					if (j.IsDestroyed || !j.IsActive)
						continue;

					j.Draw();
				}
				Raylib.EndShaderMode();

				foreach (var j in EnvironmentColliders)
				{
					if (!j.Flags.HasFlag(Wall.WallFlags.DrawOverlay))
						continue;

					wallSprite.Draw9Sliced(j.Position, j.Size + Vector2.One * 0.5f, j.Rotation, tint: Color.Black);
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

		foreach (var i in Lights)
		{
			LightingSystem.RemoveLight(i);
		}

		foreach (var i in colliderWalls)
		{
			foreach (var j in i.Value.Item1)
			{
				gameplayState.GetManager<CollisionManager>().RemoveWall(j);
			}

			if (i.Value.Item2 != null)
				LightingSystem.RemoveShadow(i.Value.Item2);
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
		_nextID++;

		onSpawn?.Invoke(obj); //set custom data here before Init() triggers
		obj.SpawnedIngame = true;
		obj.Init(gameplayState);
		obj.ModulesInit();

		toAddEntities.Add(obj);
		return obj;
	}

	public bool DespawnEntity(BaseEntity e)
	{
		return e.Despawn();
	}

	public void DrawDebug()
	{
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

		var max = NodeData.Nodes.Max(p => p.Key.ClearanceWeighted);
		foreach (var i in NodeData.Nodes)
		{
			if (!i.Key.Enabled)
				continue;

			Raylib.DrawCircleV(i.Key.Position, 1.5f, Colors.GREEN.Value(i.Key.ClearanceWeighted / max).Fade(0.6f));

			// if (i.Key.Clearance >= 1.5f)
			// Raylib.DrawCircleLinesV(i.Key.Position, i.Key.ClearanceWeighted, Colors.GREEN);

			// Raylib.DrawCircleV(i.Key.Position, 1.0f, Color.Yellow.Value(i.Key.Exposure));
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

	public T GetEntityByNameTag<T>(string nameTag) where T : BaseEntity
	{
		return Entities.FirstOrDefault(p => !p.IsDestroyed && p.NameTag == nameTag) as T;
	}

	public List<BaseEntity> GetEntitiesByGroup(string groupName)
	{
		if (!EntityGroups.ContainsKey(groupName))
			return new(); //TODO: optimize

		return EntityGroups[groupName];
	}

	public static Type? GetRegisteredEntityType(string name)
	{
		if (entityRegistry.TryGetValue($"{name}Entity", out var type))
			return type;
		return null;
	}

	public WorldMarker FindMarkerPosition(string id)
	{
		var mk = Markers.FirstOrDefault(p => p.ID == id);
		if (mk == null)
			Log.Send($"Warning: Marker '{id}' not found.");

		return mk;
	}
}