namespace Main.Gameplay.Entities;

public interface IEntityModule : IDisposable //for generic use only
{
	void Init();
	void Update(float dt, float udt);
	void LateUpdate(float dt, float udt);	
}

public abstract class EntityModule<T> : IEntityModule where T : BaseEntity
{
	protected T Entity { get; private set; }
	protected readonly GameplayState gameplayState;

	public EntityModule(GameplayState gameplayState, T entity)
	{
		this.gameplayState = gameplayState;
		Entity = entity;
	}

	public virtual void Init()
	{

	}

	public virtual void Update(float dt, float udt)
	{

	}

	public virtual void LateUpdate(float dt, float udt)
	{

	}

	public virtual void Dispose()
	{
		
	}
}