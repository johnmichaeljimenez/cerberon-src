using Main.Core;

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

	protected int HP;
	protected bool isDead;

	protected Vector2 velocity;

	[JsonIgnore]
	public CircleBody CollisionBody { get; private set; }

	public Vector2 FacingDirection => new Vector2(
		MathF.Cos(FacingAngle * MathF.PI / 180f),
		MathF.Sin(FacingAngle * MathF.PI / 180f)
	);

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		HP = MaxHP;
		CollisionBody = gameplayState.GetManager<CollisionManager>().AddBody(Position, Radius, this);
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
	}

	public override void Draw()
	{
		CurrentSprite.Draw(Position, rotation: FacingAngle, origin: new Vector2(0.25f, 0.5f));
	}

	public override void DrawDebug()
	{
		Raylib.DrawCircleLinesV(CollisionBody.Position, CollisionBody.Radius, Colors.GREEN);
		Raylib.DrawCircleLinesV(Position + (FacingDirection * 0.6f), 0.3f, Colors.GREEN);
	}

	public void ApplyDamage(int amt)
	{
		if (isDead)
			return;

		HP -= amt;
		Log.Send($"HIT: {amt} -> {HP}/{MaxHP}");
		if (HP <= 0)
		{
			HP = 0;
			isDead = true;
			OnDeath();
		}
	}

	protected virtual void OnDeath()
	{

	}
}