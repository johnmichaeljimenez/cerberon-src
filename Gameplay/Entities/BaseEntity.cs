using System.ComponentModel;
using Cerberon.Core;

namespace Cerberon.Gameplay.Entities;

public abstract class BaseEntity : IDisposable
{
	[JsonProperty]
	public int ID { get; set; }
	[JsonProperty]
	public string NameTag { get; set; }

	[JsonProperty]
	public Vector2 Position { get; set; }

	private bool _isActive = true;
	[JsonProperty]
	[DefaultValue(true)]
	public bool IsActive
	{
		get => _isActive;
		set
		{
			if (IsActive == _isActive)
				return;

			IsActive = _isActive;
			OnActiveStateChanged(IsActive);
		}
	}

	[JsonIgnore]
	public int SortingIndex { get; set; } = 0;

	private string _currentSpriteID;
	[JsonProperty]
	public string CurrentSpriteID
	{
		get => _currentSpriteID;
		set
		{
			if (_currentSpriteID == value) return;
			_currentSpriteID = value;
			CurrentSprite = AssetManager.GetSprite(value);
		}
	}

	[JsonIgnore]
	public Sprite CurrentSprite { get; protected set; }

	[JsonIgnore]
	public bool IsDestroyed { get; private set; }

	[JsonIgnore]
	protected GameplayState gameplayState { get; private set; }

	public readonly List<string> Groups = new();

	protected readonly List<IDisposable> disposables = new();

	//due to how the lifecycle works, constructor is not recommended to use for public-facing values, but it's not totally banned like in Unity

	[JsonIgnore]
	public bool SpawnedIngame { get; set; }

	[JsonIgnore]
	public virtual float SpriteMaskAmount => 1.0f;

	private readonly Dictionary<Type, IEntityModule> entityModules = new();

	public virtual void Init(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
		CurrentSprite = AssetManager.GetSprite(_currentSpriteID);
		Groups.Add(GetType().Name);
	}

	public void ModulesInit()
	{
		foreach (var i in entityModules)
		{
			i.Value.Init();
		}
	}

	public virtual void PostInit()
	{

	}

	protected T AddModule<T>() where T : IEntityModule
	{
		var module = Activator.CreateInstance(typeof(T), gameplayState, this) as IEntityModule;
		entityModules.Add(typeof(T), module);

		return (T)module;
	}

	public virtual void Update(float dt, float udt)
	{

	}

	public virtual void LateUpdate(float dt, float udt)
	{

	}

	public void ModulesUpdate(float dt, float udt)
	{
		foreach (var i in entityModules)
		{
			i.Value.Update(dt, udt);
		}
	}

	public void ModulesLateUpdate(float dt, float udt)
	{
		foreach (var i in entityModules)
		{
			i.Value.LateUpdate(dt, udt);
		}
	}

	public virtual void Draw()
	{
		//i don't see any sense in making invisible entities anyway like in Unity, like 99% of entities follow this same format overall
		CurrentSprite?.Draw(Position);
	}

	public virtual void Dispose()
	{
		foreach (var i in entityModules)
		{
			i.Value.Dispose();
		}

		disposables.ForEach(p => p?.Dispose());
	}

	public virtual bool Despawn()
	{
		if (IsDestroyed)
			return false;

		IsDestroyed = true;
		return true;
	}


	public virtual void DrawDebug()
	{

	}

	protected virtual void OnActiveStateChanged(bool isActive)
	{

	}

	public T GetModule<T>() where T : IEntityModule
	{
		return (T)entityModules[typeof(T)];
	}
}