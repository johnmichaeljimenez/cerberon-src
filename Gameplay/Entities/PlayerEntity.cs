using Main.Core;

namespace Main.Gameplay.Entities;

public class PlayerEntity : BaseEntity
{
	private float facingAngle;
	private Vector2 laserHit;

	//basic soldier with movement and direction input for now
	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
	}

	public override void Update(float dt)
	{
		base.Update(dt);

		var speed = 8f;
		var rotSpeed = 8f;
		var vel = InputManager.Movement * speed * dt;
		gameplayState.GetManager<CollisionManager>().Move(Position, 1, ref vel); //super smooth and accurate collision detection and resolution
		Position += vel;

		facingAngle = Raymath.LerpAngle(facingAngle, Position.ToDirection(InputManager.MouseWorldPosition), dt * rotSpeed);

		Game.Instance.Camera.Follow(Position, 3f);
	}

	public override void Draw()
	{
		CurrentSprite.Draw(Position, rotation: facingAngle, origin: new Vector2(0.25f, 0.5f));
	}

	public override void DrawDebug()
	{
		Raylib.DrawCircleLinesV(Position, 1, Color.Green);
	}
}