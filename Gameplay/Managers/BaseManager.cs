namespace Main.Gameplay.Managers;

public abstract class BaseManager : IDisposable
{
	//REAL, useful manager objects that are exclusively in-game, for gameplay stuff.
	//AudioHandler or SaveHandler doesn't count, as it's technically global and used outside ingame so they are in Core.
	//so far I only think of:
	// EventManager (ex. player got hit and HUD needs to be notified to update healthbar)
	// CameraManager (screenshake, bounds and framing, as main menu doesnt need those stuff)
	// PlayerManager (so everyone can reference the player character) (which already exists)
	// CollisionManager (which already exists)

	protected GameplayState gameplayState { get; private set; }
	protected readonly List<IDisposable> disposables = new();

	public BaseManager(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
	}

	public virtual void Dispose()
	{
		disposables.ForEach(p => p?.Dispose());
	}

	public virtual void Update(float dt, float udt)
	{

	}
	
	public virtual void Init()
	{

	}

	public virtual void DrawImGui()
	{

	}
}