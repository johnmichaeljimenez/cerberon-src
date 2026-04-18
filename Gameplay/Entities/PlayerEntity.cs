using Main.Core;

namespace Main.Gameplay.Entities;

public class PlayerEntity : CharacterEntity
{
	private Vector2 laserHit;

	//basic soldier with movement and direction input for now
	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
		Game.Instance.Camera.Follow(Position);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		velocity = InputManager.Movement * MovementSpeed;
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);
		
		float rotSpeed = 8;
		FacingAngle = Raymath.LerpAngle(FacingAngle, Position.ToDirection(InputManager.MouseWorldPosition), dt * rotSpeed);

		var d = 0f;
		gameplayState.GetManager<CollisionManager>().Linecast(Position, Position + (FacingDirection * 100), out d);
			laserHit = Position + (FacingDirection * d);

		Game.Instance.Camera.Follow(Position, 3f);
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawLineV(Position, laserHit, Color.Red);
		Raylib.DrawCircleV(laserHit, 0.3f, Color.Red);
	}
}