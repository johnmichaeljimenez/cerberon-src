using Main.Core;

namespace Main.Gameplay.Entities;

public class PlayerEntity : BaseEntity
{
	private float facingAngle;

	//basic soldier with movement and direction input for now (no need for collision as it's just simple entity test)
	public override void Init()
	{
		base.Init();

		CurrentSpriteID = "soldier";
	}

	public override void Update(float dt)
	{
		base.Update(dt);

		var speed = 8f;
		var rotSpeed = 8f;
		Position += InputManager.GetMovement() * speed * dt;

		facingAngle = Raymath.LerpAngle(facingAngle, Position.ToDirection(InputManager.MouseWorldPosition) + 90, dt * rotSpeed); //+90 is temporary, will use true 0-degree neutral angle (+X) on sprites

		if (InputManager.IsKeyDown(KeyboardKey.Space))
			Game.Instance.Camera.Follow(Position, 3f);
	}

	public override void Draw()
	{
		CurrentSprite.Draw(Position, rotation: facingAngle, origin: new Vector2(0.45f, 0.75f));
	}
}