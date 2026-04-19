using Main.Core;
using Main.Effects;

namespace Main.Gameplay.Entities;

public class Gun
{
	public string Name;
	public int MaxAmmo;
	public int MagSize;
	public float FiringRate; // FiringRate <= 0 means tap to shoot
	public int Damage;
	public float ReloadTime;

	public int CurrentAmmo;
	public int CurrentMaxAmmo;

	public bool CanReload()
	{
		return CurrentAmmo < MagSize && CurrentMaxAmmo > 0;
	}

	public void DoReload()
	{
		if (!CanReload()) return;

		int ammoToAdd = Math.Min(MagSize - CurrentAmmo, CurrentMaxAmmo);
		CurrentAmmo += ammoToAdd;
		CurrentMaxAmmo -= ammoToAdd;
	}
}

public class PlayerEntity : CharacterEntity
{
	public readonly List<Gun> guns = new() //total hardcoded for now
	{
		new Gun()
		{
			Name = "Sig Sauer",
			Damage = 15,
			FiringRate = 0f, // FiringRate <= 0 means tap to shoot
			MagSize = 15,
			MaxAmmo = 60,
			ReloadTime = 1.4f
		},

		new Gun()
		{
			Name = "AK-47",
			Damage = 30,
			FiringRate = 0.15f,
			MagSize = 30,
			MaxAmmo = 120,
			ReloadTime = 2
		}
	};

	private LinecastHit laserHit;
	private Light lightSelf;
	private Light flashLight;

	private Light muzzleFlash;
	private bool flashLightOn;

	private int currentWeaponIndex;
	private float fireTimer;
	private float reloadTimer;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		CurrentSpriteID = "soldier";
		Game.Instance.Camera.Follow(Position);
		lightSelf = LightingSystem.AddLight(AssetManager.GetSprite("light"), Position, Color.DarkGray, 0, 10);
		flashLight = LightingSystem.AddLight(AssetManager.GetSprite("flashlight"), Position, Color.White, FacingAngle, 10, flashLightOn, new(0f, 0.5f));
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		velocity = InputManager.Movement * MovementSpeed;

		if (muzzleFlash != null)
		{
			if (muzzleFlash.Color.A > 0)
			{
				muzzleFlash.Color = muzzleFlash.Color.Fade((muzzleFlash.Color.A / 255f) * 0.9f); //quick "exponential" fade test
				if (muzzleFlash.Color.A <= 0)
				{
					LightingSystem.RemoveLight(muzzleFlash);
					muzzleFlash = null;
				}
			}
		}

		gameplayState.GetManager<CollisionManager>().Linecast(Position, Position + (FacingDirection * 100), out laserHit, CollisionBody);

		if (InputManager.FlashlightJustPressed)
		{
			flashLightOn = !flashLightOn;
			flashLight.Enabled = flashLightOn;
		}

		if (InputManager.ActionJustPressed)
		{
			Log.Send("Shoot");

			if (muzzleFlash != null)
			{
				LightingSystem.RemoveLight(muzzleFlash);
			}

			muzzleFlash = LightingSystem.AddLight(AssetManager.GetSprite("light"), Position, new(80, 30, 0), 0, 14);

			if (laserHit.Body != null && laserHit.Body.SourceEntity is ZombieEntity z)
				z.ApplyDamage(40);
		}
	}

	public override void LateUpdate(float dt, float udt)
	{
		base.LateUpdate(dt, udt);

		float rotSpeed = 12;
		FacingAngle = Raymath.LerpAngle(FacingAngle, Position.ToDirection(InputManager.MouseWorldPosition), dt * rotSpeed);

		lightSelf.Position = Position;
		flashLight.Position = Position;
		flashLight.Rotation = Raymath.LerpAngle(flashLight.Rotation, FacingAngle, dt * rotSpeed); //intentional delay

		Game.Instance.Camera.Follow(Position, 3f);
	}

	public override void Draw()
	{
		base.Draw();

		Raylib.DrawLineV(Position, laserHit.HitPosition, Colors.RED.Fade(laserHit.Body != null ? 1 : 0.4f));

		if (laserHit.Body != null)
			Raylib.DrawCircleV(laserHit.HitPosition, 0.3f, Colors.RED);
	}

	public override void Dispose()
	{
		base.Dispose();

		if (muzzleFlash != null)
			LightingSystem.RemoveLight(muzzleFlash);

		LightingSystem.RemoveLight(flashLight);
		LightingSystem.RemoveLight(lightSelf);
	}
}