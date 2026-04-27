using Main.Core;
using Main.Gameplay.Entities.Player;

namespace Main.Gameplay.Managers;

public class PlayerManager : BaseManager
{
	public PlayerEntity PlayerCharacter { get; private set; }

	public readonly Signal<PlayerEntity> OnPlayerDeath = new();

	public PlayerManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void SpawnPlayer(Vector2 position)
	{
		PlayerCharacter = gameplayState.CurrentWorld.SpawnEntity<PlayerEntity>((e) =>
		{
			e.Position = position;
		});
	}
}