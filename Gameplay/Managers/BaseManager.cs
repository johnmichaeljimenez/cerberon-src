namespace Main.Gameplay.Managers;

public abstract class BaseManager : IDisposable
{
	//i am not sure what are the other REAL, common Managers that I will implement aside from the one below
	//basically, manager objects that are exclusively in-game, for gameplay stuff.
	//AudioHandler or SaveHandler doesn't count, as it's technically global and used outside ingame
	//so far I only think of:
	// PlayerManager (so everyone can reference the player character)
	// EventManager (ex. player got hit and HUD needs to be notified to update healthbar)
	// CameraManager (bounds and framing, as main menu doesnt need those stuff)
	// CollisionManager (which already exists)

	public BaseManager()
	{

	}

	public void Dispose()
	{

	}

	public void Update()
	{

	}

	public void Draw()
	{

	}
}