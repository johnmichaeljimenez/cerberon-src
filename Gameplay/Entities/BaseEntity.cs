using Main.Core;

namespace Main.Gameplay;

public abstract class BaseEntity : IDisposable
{
	[JsonProperty]
	public int ID { get; set; }

	[JsonProperty]
	public Vector2 Position { get; set; }

	[JsonProperty]
	public bool IsActive { get; set; }

	[JsonProperty]
	public Sprite CurrentSprite { get; set; }

	public bool IsDestroyed { get; private set; }

	public BaseEntity()
	{

	}

	public virtual void Update(float dt)
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
}