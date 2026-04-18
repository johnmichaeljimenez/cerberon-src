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
	public float MovementSpeed { get; set; } = 8.0f;
	[JsonProperty]
	public float Radius { get; set; } = 1.0f;

	[JsonIgnore]
	public float FacingAngle { get; set; }

	protected Vector2 velocity;

	public Vector2 FacingDirection => new Vector2(
		MathF.Cos(FacingAngle * MathF.PI / 180f),
		MathF.Sin(FacingAngle * MathF.PI / 180f)
	);

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		if (velocity.LengthSquared() > 0)
		{
			var vel = velocity * dt;
			gameplayState.GetManager<CollisionManager>().Move(Position, Radius, ref vel); //super smooth and accurate collision detection and resolution
			Position += vel;
		}
	}

	public override void Draw()
	{
		CurrentSprite.Draw(Position, rotation: FacingAngle, origin: new Vector2(0.25f, 0.5f));
	}

	public override void DrawDebug()
	{
		Raylib.DrawCircleLinesV(Position, 1, Color.Green);
		Raylib.DrawCircleLinesV(Position + (FacingDirection * 0.6f), 0.3f, Color.Green);
	}
}