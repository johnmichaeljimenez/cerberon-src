using Main.Core;
using Main.Gameplay.Managers;

namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity
{
	public override Teams Team => Teams.Enemy;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
		MovementSpeed = 4.0f;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		//dumb movement (they are zombies anyway)
		var d = gameplayState.GetManager<PlayerManager>().PlayerCharacter.Position - Position;
		if (d.Length() <= 1.5f)
			velocity = Raymath.Vector2Lerp(velocity, Vector2.Zero, dt * 10);
		else
			velocity = Raymath.Vector2Lerp(velocity, Raymath.Vector2Normalize(d) * MovementSpeed, dt * 10);

		FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		Despawn();
	}
}