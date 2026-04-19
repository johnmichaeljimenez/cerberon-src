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
	public float MovementSpeed { get; private set; } = 8.0f;
	[JsonProperty]
	public float Radius { get; private set; } = 1.0f;
	[JsonProperty]
	public int MaxHP { get; private set; } = 100;

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
		CollisionBody = new()
		{
			Position = Position,
			Radius = Radius
		};
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		if (velocity.LengthSquared() > 0)
		{
			var cm = gameplayState.GetManager<CollisionManager>();
			var vel = velocity * dt;
			cm.Move(CollisionBody, ref vel); //super smooth and accurate collision detection and resolution
			cm.Move(CollisionBody, gameplayState.CurrentWorld.CharacterEntities.Select(p => p.CollisionBody), ref vel); //Select is temporary
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