using Main.Core;
using Main.Effects;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class EnemyEntity : CharacterEntity
{
	[DataConfig(70f)] static float STATS_BASE_HP;
	[DataConfig(50)] static int STATS_OUTDOOR_LIGHT_DAMAGE;
	[DataConfig(2f)] static float STATS_OUTDOOR_LIGHT_INTERVAL;

	[DataConfig(0.8f)] static float STATS_BASE_RADIUS;

	[DataConfig(10)] static int STATS_BASE_DAMAGE;

	[DataConfig(7)] static float STATS_BASE_MOVEMENT_SPEED;

	const float LIFETIME = 20f;


	public override Teams Team => Teams.Enemy;

	[JsonIgnore]
	public bool Persistent { get; set; }
	[JsonProperty]
	public bool IsFlyer { get; set; } = false;
	[JsonProperty]
	public float Cost { get; set; } = 0.5f;

	private bool flying;
	private Vector2 flyTarget;
	private float visibleTime = 0;
	private int attackRequestIndex = -1;
	private float lifetime; //despawn far-away enemies after certain time
	private float fsTimer = 0;

	private readonly List<Vector2> nodes = new();

	private int attackDamage;
	private float normalizedCost;

	private float outdoorTimer;

	public override void Init(GameplayState gameplayState)
	{
		normalizedCost = Raymath.Remap(Cost, 0f, 2f, 2f, 0.5f);
		lifetime = LIFETIME;
		attackRequestIndex = -1;
		SetStats();

		base.Init(gameplayState);
		Animator.BaseSpeed = normalizedCost;

		Animator.Add("roach-idle", 0);
		Animator.Add("roach-move", 0);
		Animator.Add("roach-fly", 0);
		Animator.Add("roach-attack", 50);
		Animator.Add("roach-death", 100);
		Animator.Play("roach-idle");

		Log.Send($"Spawned enemy #{ID}");
	}

	private void SetStats()
	{
		Scale = Math.Max(0.1f, Cost);
		MaxHP = Math.Max(1, (int)(STATS_BASE_HP * Cost));
		Radius = STATS_BASE_RADIUS;
		MovementSpeed = STATS_BASE_MOVEMENT_SPEED * normalizedCost;
		attackDamage = Math.Max(1, (int)(STATS_BASE_DAMAGE * Cost));

		Origin = new(0.4f, 0.5f);
	}

	protected override void OnAnimationEnd(string animationName)
	{
		if (animationName == "roach-attack")
		{
			ReleaseAttack();
			return;
		}

		if (animationName == "roach-death")
		{
			DecalSystem.PaintDead(CurrentSprite, Position, FacingAngle, Origin, Scale);
			Despawn();
		}
	}

	protected override void OnAnimationFrameChanged((string, int, float) frameData)
	{
		if (frameData.Item1 != "roach-attack" || frameData.Item2 != 6) //guaranteed to be frame-perfect than using normalized time
			return;

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = player.Position - Position;
		if (!player.IsDead && FacingDirection.IsInFront(player.Position - Position, Raymath.Lerp(3, 6, Cost / 2), 50))
		{
			player.ApplyDamage(attackDamage, this);
		}
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (IsDead)
			return;

		var player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;
		var d = player.Position - Position;

		if (flying)
		{
			d = flyTarget - Position;
			FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
			velocity = Vector2.Normalize(d) * MovementSpeed * 2;
			CollisionBody.Height = CollisionHeight.High;

			if (d.Length() <= 2)
			{
				flying = false;
				CollisionBody.Height = CollisionHeight.Mid;
			}

			return;
		}

		if (IsAnimatorBusy)
		{
			FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);
			velocity = Raymath.Vector2Lerp(velocity, Vector2.Zero, dt * 10);
			return;
		}

		var dist = d.Length();
		if (dist <= 3f)
		{
			visibleTime += dt;
			lifetime = LIFETIME;
			nodes.Clear();

			ReleaseAttack();
			attackRequestIndex = gameplayState.GetManager<AIDirectorManager>().RequestAttack();

			if (attackRequestIndex >= 0)
			{
				if (Animator.Play("roach-attack", false, "roach-idle"))
				{
					velocity = Vector2.Zero;
				}
			}
		}
		else
		{
			var w = gameplayState.GetManager<WaypointManager>();
			if (w.IsVisible(Position, player.Position)) //go straight to player if directly visible (not true FOV yet)
			{
				visibleTime += dt;
				lifetime = LIFETIME;
				if (nodes.Count > 0)
					nodes.Clear();
			}
			else
			{
				visibleTime = 0;
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


			if (IsFlyer && d.Length() >= 8)
			{
				flying = true;
				flyTarget = Position + (d * 0.5f); //undershoot flying target
				AudioHandler.PlaySound("roach/fly", Position);
				return;
			}

			velocity = Raymath.Vector2Lerp(velocity, Raymath.Vector2Normalize(d) * MovementSpeed, dt * 10);
		}

		FacingAngle = Raymath.LerpAngle(FacingAngle, d.ToDirection(), dt * 8);

		if (gameplayState.CurrentWorld.NodeData.IsOutdoor(NearestNode, true))
		{
			if (Utils.Countdown(ref outdoorTimer, dt))
			{
				ApplyDamage(STATS_OUTDOOR_LIGHT_DAMAGE, null);
				outdoorTimer = RNG.Range(0.8f, 1.2f) * STATS_OUTDOOR_LIGHT_INTERVAL;
			}
		}
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		if (IsAnimatorBusy)
			return;

		var ms = MovementSpeed / 1.5f;
		if (velocity.LengthSquared() > ms * ms)
		{
			if (flying)
			{
				Animator.Play("roach-fly");
			}
			else
			{
				Animator.Play("roach-move");
				fsTimer += dt;

				if (fsTimer >= 0.4f)
				{
					AudioHandler.PlaySound("fs/rock", Position);
					fsTimer = 0;
				}
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

	// public override void Draw()
	// {
	// 	RenderingManager.BeginMaskedShader();
	// 	base.Draw();
	// 	Raylib.EndShaderMode();
	// }

	private void ReleaseAttack()
	{
		if (attackRequestIndex >= 0)
		{
			gameplayState.GetManager<AIDirectorManager>().ReleaseAttack(attackRequestIndex);
		}

		attackRequestIndex = -1;
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