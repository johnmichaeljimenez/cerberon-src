using Main.Core;
using Main.Gameplay.Managers;

namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity
{
	public override Teams Team => Teams.Enemy;

	//temporary stuff for testing
	private float attackRate = 2;
	private int attackDamage = 20;
	private float attackTimer;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		Animator = new Animator("zombie-idle", "zombie-move");
		MovementSpeed = 4.0f;
		attackTimer = attackRate;
		
		Animator.Play("zombie-idle");
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		//dumb movement (they are zombies anyway)
		var d = player.Position - Position;
		if (d.Length() <= 2f)
		{
			if (!player.IsDead && Utils.Countdown(ref attackTimer, dt))
			{
				attackTimer = attackRate;
				player.ApplyDamage(attackDamage);
			}

			velocity = Raymath.Vector2Lerp(velocity, Vector2.Zero, dt * 10);
		}
		else
		{
			velocity = Raymath.Vector2Lerp(velocity, Raymath.Vector2Normalize(d) * MovementSpeed, dt * 10);
		}

		FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);
		
		if (velocity.LengthSquared() > 0.1f)
			Animator.Play("zombie-move");
		else
			Animator.Play("zombie-idle");
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		Despawn();
	}
}