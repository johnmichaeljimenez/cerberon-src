using Main.Gameplay.Entities;

namespace Main.Gameplay.Managers;

public class PlayerManager : BaseManager
{
	public PlayerEntity PlayerCharacter { get; private set; }

	public PlayerManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void SpawnPlayer(Vector2 position)
	{
		PlayerCharacter = gameplayState.CurrentWorld.SpawnEntity<PlayerEntity>();
		PlayerCharacter.Position = position;
	}
}