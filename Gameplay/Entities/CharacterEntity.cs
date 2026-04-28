using Main.Core;
using Main.Effects;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public abstract class CharacterEntity : BaseEntity //used by player, enemy, npc (real npc, not the interactive static mannequin npcs like in other games). all of them share a lot of stuff here
{
	public enum Teams
	{
		Player = 0,
		Enemy = 1
	}

	[JsonIgnore]
	public virtual Teams Team => Teams.Player;
	[JsonProperty]
	public float MovementSpeed { get; protected set; } = 8.0f;
	[JsonProperty]
	public float Radius { get; protected set; } = 1.0f;
	[JsonProperty]
	public int MaxHP { get; protected set; } = 100;

	[JsonIgnore]
	public float FacingAngle { get; set; }

	[JsonIgnore]
	public int HP { get; private set; }
	[JsonIgnore]
	public bool IsDead { get; private set; }

	protected Vector2 velocity;

	[JsonIgnore]
	public CircleBody CollisionBody { get; private set; }

	public Animator Animator { get; protected set; }

	[JsonIgnore]
	public Vector2 Origin { get; set; } = Vector2.One * 0.5f;

	public Vector2 FacingDirection => new Vector2(
		MathF.Cos(FacingAngle * MathF.PI / 180f),
		MathF.Sin(FacingAngle * MathF.PI / 180f)
	);

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		HP = MaxHP;
		CollisionBody = gameplayState.GetManager<CollisionManager>().AddBody(Position, Radius, this);

		Animator = new Animator();
		Animator.OnAnimationBegin.Subscribe(OnAnimationBegin).AddTo(disposables);
		Animator.OnAnimationEnd.Subscribe(OnAnimationEnd).AddTo(disposables);
		Animator.OnFrameChanged.Subscribe(OnAnimationFrameChanged).AddTo(disposables);
	}

	protected virtual void OnAnimationEnd(string animationName)
	{
		
	}

	protected virtual void OnAnimationFrameChanged((string, int, float) frameData)
	{
		
	}

	protected virtual void OnAnimationBegin(string animationName)
	{
		
	}

	public override void Dispose()
	{
		base.Dispose();

		gameplayState.GetManager<CollisionManager>().RemoveBody(CollisionBody);
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		if (velocity.LengthSquared() > 0)
		{
			var cm = gameplayState.GetManager<CollisionManager>();
			var vel = velocity * dt;
			cm.Move(CollisionBody, ref vel); //super smooth and accurate collision detection and resolution
			cm.MoveRepelBody(CollisionBody, ref vel);
			Position += vel;
			CollisionBody.Position = Position;
		}

		if (Animator != null)
		{
			Animator.Update(dt, udt);
			var spr = Animator.GetFrameSprite();
			if (spr != null)
				CurrentSprite = Animator.GetFrameSprite();
		}
	}

	public override void Draw()
	{
		CurrentSprite?.Draw(Position, rotation: FacingAngle, origin: Origin);
	}

	public override void DrawDebug()
	{
		Raylib.DrawCircleLinesV(CollisionBody.Position, CollisionBody.Radius, Colors.GREEN);
		Raylib.DrawCircleLinesV(Position + (FacingDirection * 0.6f), 0.3f, Colors.GREEN);
	}

	public bool Heal(int amt)
	{
		if (IsDead)
			return false;

		HP += amt;
		HP = Math.Min(HP, MaxHP);
		return true;
	}

	public bool ApplyDamage(int amt)
	{
		if (IsDead)
			return false;

		DecalSystem.Paint(Position);
		HP -= amt;
		Log.Send($"HIT: {amt} -> {HP}/{MaxHP}");
		if (HP <= 0)
		{
			HP = 0;
			IsDead = true;
			OnDeath();
		}

		return true;
	}

	protected virtual void OnDeath()
	{
		DecalSystem.Paint(Position);
	}
}