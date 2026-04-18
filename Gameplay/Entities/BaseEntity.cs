using System.Runtime.Serialization;
using Main.Core;

namespace Main.Gameplay.Entities;

public abstract class BaseEntity : IDisposable
{
	[JsonProperty]
	public int ID { get; set; }

	[JsonProperty]
	public Vector2 Position { get; set; }

	[JsonProperty]
	public bool IsActive { get; set; } = true;

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
	public Sprite CurrentSprite { get; private set; }

	[JsonIgnore]
	public bool IsDestroyed { get; private set; }

	[JsonIgnore]
	protected GameplayState gameplayState { get; private set; }

	public virtual void Init(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
	}

	public virtual void Update(float dt, float udt)
	{

	}

	public virtual void LateUpdate(float dt, float udt)
	{

	}

	public virtual void Draw()
	{
		//i don't see any sense in making invisible entities anyway like in Unity, like 99% of entities follow this same format overall
		CurrentSprite?.Draw(Position);
	}

	public virtual void Dispose()
	{

	}

	public virtual bool Despawn()
	{
		if (IsDestroyed)
			return false;

		IsDestroyed = true;
		Dispose();

		return true;
	}

	[OnDeserialized]
	protected virtual void OnDeserialized(StreamingContext _)
	{
		CurrentSprite = AssetManager.GetSprite(_currentSpriteID);
	}

	public virtual void DrawDebug()
	{
		
	}
}