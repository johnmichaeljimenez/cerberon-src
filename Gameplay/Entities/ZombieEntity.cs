using Main.Core;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class ZombieEntity : CharacterEntity
{
	public override Teams Team => Teams.Enemy;
	private int attackDamage = 20;

	private readonly List<Vector2> nodes = new();

	private float fsTimer = 0;
	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		Animator.Add("zombie-idle", 0);
		Animator.Add("zombie-move", 0);
		Animator.Add("zombie-attack", 50);

		MovementSpeed = 4.0f;

		Animator.Play("zombie-idle");
	}

	protected override void OnAnimationFrameChanged((string, int, float) frameData)
	{
		if (frameData.Item1 != "zombie-attack" || frameData.Item2 != 5) //guaranteed to be frame-perfect than using normalized time
			return;

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = player.Position - Position;
		if (!player.IsDead && d.Length() <= 4f)
		{
			player.ApplyDamage(attackDamage);
		}
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = player.Position - Position;

		if (IsAnimatorBusy)
		{
			FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
			velocity = Raymath.Vector2Lerp(velocity, Vector2.Zero, dt * 10);
			return;
		}

		if (d.Length() <= 3f)
		{
			nodes.Clear();

			if (Animator.Play("zombie-attack", false, "zombie-idle"))
			{
				velocity = Vector2.Zero;
			}
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

		if (IsAnimatorBusy)
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

	public override void Draw()
	{
		RenderingManager.BeginMaskedShader();
		base.Draw();
		Raylib.EndShaderMode();
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