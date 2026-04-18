namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity
{
	public override Teams Team => Teams.Enemy;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
	}
}