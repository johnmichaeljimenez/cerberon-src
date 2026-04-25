using Main.Core;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity
{
	public override Teams Team => Teams.Enemy;

	//temporary stuff for testing
	private float attackRate = 2;
	private int attackDamage = 20;
	private float attackTimer;

	private readonly List<Vector2> nodes = new();

	private float fsTimer = 0;
	private bool isAttacking; //TODO: use proper FSM later

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		Animator = new Animator("zombie-idle", "zombie-move", "zombie-attack");
		MovementSpeed = 4.0f;
		attackTimer = attackRate;

		Animator.Play("zombie-idle");

		Animator.OnAnimationEnd = (animation) =>
		{
			if (animation != "zombie-attack")
				return;

			isAttacking = false;
		};

		Animator.OnFrameChanged = (animation, index, t) =>
		{
			if (animation != "zombie-attack" || index != 5) //guaranteed to be frame-perfect than using normalized time
				return;

			var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
			var d = player.Position - Position;
			if (!player.IsDead && d.Length() <= 3f)
			{
				attackTimer = attackRate;
				player.ApplyDamage(attackDamage);
			}
		};
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = player.Position - Position;

		if (isAttacking)
		{
			FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
			return;
		}

		if (d.Length() <= 4f)
		{
			nodes.Clear();
			velocity = Raymath.Vector2Lerp(velocity, Vector2.Zero, dt * 10);

			isAttacking = true;
			velocity = Vector2.Zero;
			Animator.Play("zombie-attack");
		}
		else
		{
			var w = gameplayState.GetManager<WaypointManager>();
			if (w.IsVisible(Position, player.Position)) //go straight to player if directly visible (not true FOV yet)
			{
				if (nodes.Count > 0)
					nodes.Clear();
			}
			else
			{
				if (nodes.Count == 0) //change the path only when the current path is reached for immersion, but will add cooldown for frequency control
				{
					w.Move(Position, player.Position, nodes);
				}

				if (nodes.Count > 0)
				{
					var nd = nodes[0] - Position;
					if (nd.Length() <= 2f)
					{
						nodes.RemoveAt(0);
						if (nodes.Count == 0)
						{
							w.Move(Position, player.Position, nodes);
						}
						else
						{
							nd = nodes[0] - Position;
						}
					}

					d = nd;
				}
			}

			velocity = Raymath.Vector2Lerp(velocity, Raymath.Vector2Normalize(d) * MovementSpeed, dt * 10);
		}

		FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);
		if (isAttacking)
			return;

		if (velocity.LengthSquared() > 0.1f)
		{
			Animator.Play("zombie-move");
			fsTimer += dt;

			if (fsTimer >= 0.4f)
			{
				AudioHandler.PlaySound("fs/rock", Position);
				fsTimer = 0;
			}
		}
		else
		{
			Animator.Play("zombie-idle");
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		Despawn();
	}

	// public override void DrawDebug()
	// {
	// 	base.DrawDebug();

	// 	for (int i = 0; i < nodes.Count - 1; i++)
	// 	{
	// 		Raylib.DrawLineEx(nodes[i], nodes[i + 1], 2, Colors.YELLOW);
	// 	}
	// }
}