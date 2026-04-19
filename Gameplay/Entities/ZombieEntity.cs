namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity //TODO: add character-character collision resolution (at least a repel effect)
{
	public override Teams Team => Teams.Enemy;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		Despawn();
	}
}