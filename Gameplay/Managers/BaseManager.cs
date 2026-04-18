namespace Main.Gameplay.Managers;

public abstract class BaseManager : IDisposable
{
	//REAL, useful manager objects that are exclusively in-game, for gameplay stuff.
	//AudioHandler or SaveHandler doesn't count, as it's technically global and used outside ingame so they are in Core.
	//so far I only think of:
	// PlayerManager (so everyone can reference the player character)
	// EventManager (ex. player got hit and HUD needs to be notified to update healthbar)
	// CameraManager (bounds and framing, as main menu doesnt need those stuff)
	// CollisionManager (which already exists)

	protected GameplayState gameplayState { get; private set; }

	public BaseManager(GameplayState gameplayState)
	{
		this.gameplayState = gameplayState;
	}

	public virtual void Dispose()
	{

	}

	public virtual void Update(float dt)
	{

	}
	
	public virtual void Init()
	{

	}

	public virtual void DrawImGui()
	{

	}
}