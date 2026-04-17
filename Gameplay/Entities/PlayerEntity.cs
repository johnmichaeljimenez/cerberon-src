using Main.Core;

namespace Main.Gameplay.Entities;

public class PlayerEntity : BaseEntity
{
	public override void Init()
	{
		base.Init();

		CurrentSpriteID = "birdie";
	}

	public override void Update(float dt)
	{
		base.Update(dt);

		//basic movement for now (no need for collision as it's just simple entity test)
		var speed = 0.1f;
		Position += InputManager.GetMovement() * speed; //no delta time needed, values mirror real result (0.1 units or 6.4 pixels per second).
		// i might ditch dt parameter later (or maybe I can use it for timer increments as unit of time in this frame?)

		if (InputManager.IsKeyDown(KeyboardKey.Space))
			Game.Instance.Camera.Follow(Position, 3f);
	}
}