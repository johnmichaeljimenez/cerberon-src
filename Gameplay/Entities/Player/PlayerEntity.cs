using Main.Core;
using Main.Effects;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay.Entities.Player;

//put all of them here for now, component architecture is a tomorrow's problem if i can mow down zombies right now with this code. if this is a god class then call this project mt. olympus for now
//UPDATE: added PlayerWeapons.cs (moved weapon-related stuff there) it can be considered as component now, but from now on each component that I will make must be "deserving immediately now" of being a component. otherwise they will stay for now in each of their own entity classes.
public class PlayerEntity : CharacterEntity
{
	private Light lightSelf;
	private Light lightSelfVision;
	private Light flashLight;

	private bool flashLightOn;
	private bool nightvisionOn;

	private float fsTimer = 0; //test

	public PlayerWeapons Weapons { get; private set; }

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		Origin = new Vector2(0.3f, 0.7f);
		Weapons = new(gameplayState, this);

		Game.Instance.Camera.Follow(Position);
		lightSelf = LightingSystem.AddLight("light", Position, new(30, 30, 30), 0, 8);
		lightSelfVision = LightingSystem.AddLight("vision-cone", Position, Color.White, FacingAngle, 4, true, new(0.15f, 0.5f), Light.ShadowTypes.Dynamic, Light.VisionEffects.VisionOnly);
		flashLight = LightingSystem.AddLight("flashlight", Position, Color.White.Value(0.5f), FacingAngle, 10, flashLightOn, new(0f, 0.5f), Light.ShadowTypes.Dynamic); //redundant shadow but it is what it is

		Animator.Play(Weapons.CurrentWeapon.ANIM_IDLE);
	}

	protected override void OnAnimationBegin(string animationName)
	{

	}

	protected override void OnAnimationEnd(string animationName)
	{
		Weapons.OnAnimationEnd(animationName);
	}

	protected override void OnAnimationFrameChanged((string, int, float) frameData)
	{
		Weapons.OnFrameChanged(frameData);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		velocity = InputManager.Movement * MovementSpeed;

		if (InputManager.IsPressed(InputAction.Flashlight))
		{
			AudioHandler.PlaySound("generic/flashlight-toggle");
			flashLightOn = !flashLightOn;
			flashLight.Enabled = flashLightOn;
		}

		if (InputManager.IsPressed(InputAction.Nightvision))
		{
			nightvisionOn = !nightvisionOn;
			RenderingManager.SetFilter(RenderingManager.Filters.Nightvision, nightvisionOn);

			if (nightvisionOn)
			{
				AudioHandler.PlaySound("generic/nightvision-on"); //I used charging sound from a camera flash capacitor
			}
		}

		Weapons.Update(dt, udt);
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		float rotSpeed = 12;
		FacingAngle = Raymath.LerpAngle(FacingAngle, Position.ToDirection(InputManager.MouseWorldPosition), dt * rotSpeed);

		lightSelf.Position = Position;
		lightSelfVision.Position = Position;
		lightSelfVision.Rotation = FacingAngle;
		flashLight.Position = Position;
		flashLight.Rotation = Raymath.LerpAngle(flashLight.Rotation, FacingAngle, dt * rotSpeed); //intentional delay

		if (velocity.LengthSquared() > 0.1f)
		{
			Animator.Play(Weapons.CurrentWeapon.ANIM_MOVE);
			fsTimer += dt;

			if (fsTimer >= 0.4f)
			{
				AudioHandler.PlaySound("fs/rock");
				fsTimer = 0;
			}
		}
		else
		{
			Animator.Play(Weapons.CurrentWeapon.ANIM_IDLE);
		}

		var target = InputManager.MouseWorldPosition - Position;
		target = Position + Raymath.Vector2ClampValue(target, 0, 5);
		Game.Instance.Camera.Follow(target, 2f);
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawLineV(Position, Weapons.LaserHit.HitPosition, Colors.RED.Fade(Weapons.LaserHit.Body != null ? 1 : 0.4f));

		if (Weapons.LaserHit.Body != null)
			Raylib.DrawCircleV(Weapons.LaserHit.HitPosition, 0.3f, Colors.RED);
	}

	public override void Dispose()
	{
		base.Dispose();

		Weapons.Dispose();

		LightingSystem.RemoveLight(flashLight);
		LightingSystem.RemoveLight(lightSelfVision);
		LightingSystem.RemoveLight(lightSelf);
	}

	protected override void OnHit(float amt, bool isDead)
	{
		base.OnHit(amt, isDead);
		AudioHandler.PlaySound("generic/player-hit");
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		IsActive = false; //TODO: spawn a player death animation false entity

		gameplayState.GetManager<PlayerManager>().OnPlayerDeath.Publish(this);
	}
}