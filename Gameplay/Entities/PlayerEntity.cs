using Main.Core;
using Main.Effects;

namespace Main.Gameplay.Entities;

public class PlayerEntity : CharacterEntity
{
	private Vector2 laserHit;
	private Light lightSelf;
	private Light flashLight;

	//basic soldier with movement and direction input for now
	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
		Game.Instance.Camera.Follow(Position);
		lightSelf = LightingSystem.AddLight(AssetManager.GetSprite("light"), Position, Color.DarkGray, 0, 10);
		flashLight = LightingSystem.AddLight(AssetManager.GetSprite("flashlight"), Position, Color.DarkGray, FacingAngle, 10, true, new(0f, 0.5f));
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

		lightSelf.Position = Position;
		flashLight.Position = Position;
		flashLight.Rotation = Raymath.LerpAngle(flashLight.Rotation, FacingAngle, dt * 6);
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawLineV(Position, laserHit, Color.Red);
		Raylib.DrawCircleV(laserHit, 0.3f, Color.Red);
	}

	public override void Dispose()
	{
		base.Dispose();

		LightingSystem.RemoveLight(flashLight);
		LightingSystem.RemoveLight(lightSelf);
	}
}