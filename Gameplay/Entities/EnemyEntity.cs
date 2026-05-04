using Main.Core;
using Main.Effects;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class EnemyEntity : CharacterEntity
{
	const float LIFETIME = 20f;
	public override Teams Team => Teams.Enemy;
	private int attackDamage = 5;

	private readonly List<Vector2> nodes = new();

	[JsonIgnore]
	public bool Persistent { get; set; }

	private float fsTimer = 0;

	private float lifetime; //despawn far-away enemys after certain time

	public override void Init(GameplayState gameplayState)
	{
		lifetime = LIFETIME;
		MaxHP = 70;
		Radius = 0.8f;
		MovementSpeed = 4.0f;
		Log.Send($"Spawned enemy #{ID}");
		Origin = new(0.4f, 0.5f);

		base.Init(gameplayState);

		Animator.Add("roach-idle", 0);
		Animator.Add("roach-move", 0);
		Animator.Add("roach-attack", 50);
		Animator.Add("roach-death", 100);

		Animator.Play("roach-idle");
	}

	protected override void OnAnimationEnd(string animationName)
	{
		if (animationName == "roach-death")
		{
			DecalSystem.PaintDead(CurrentSprite, Position, FacingAngle, Origin);
			Despawn();
		}
	}

	protected override void OnAnimationFrameChanged((string, int, float) frameData)
	{
		if (frameData.Item1 != "roach-attack" || frameData.Item2 != 6) //guaranteed to be frame-perfect than using normalized time
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

		if (IsDead)
			return;

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
			lifetime = LIFETIME;
			nodes.Clear();

			if (Animator.Play("roach-attack", false, "roach-idle"))
			{
				velocity = Vector2.Zero;
			}
		}
		else
		{
			var w = gameplayState.GetManager<WaypointManager>();
			if (w.IsVisible(Position, player.Position)) //go straight to player if directly visible (not true FOV yet)
			{
				lifetime = LIFETIME;
				if (nodes.Count > 0)
					nodes.Clear();
			}
			else
			{
				if (d.Length() <= 10)
					lifetime = LIFETIME;

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

		var ms = MovementSpeed / 1.5f;
		if (velocity.LengthSquared() > ms * ms)
		{
			Animator.Play("roach-move");
			fsTimer += dt;

			if (fsTimer >= 0.4f)
			{
				AudioHandler.PlaySound("fs/rock", Position);
				fsTimer = 0;
			}
		}
		else
		{
			Animator.Play("roach-idle");
		}

		if (!Persistent)
		{
			lifetime -= dt;
			if (lifetime <= 0)
			{
				Despawn();
				Log.Send($"Despawned enemy #{ID}");
			}
		}
	}

	protected override void OnDeath()
	{
		base.OnDeath();

		Animator.Play("roach-death");
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