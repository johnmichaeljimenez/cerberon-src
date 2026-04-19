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



	public Gun(string name, int damage, float firingRate,
			   int magSize, int maxAmmo, float reloadTime)
	{
		Name = name;
		Damage = damage;
		FiringRate = firingRate;
		MagSize = magSize;
		MaxAmmo = maxAmmo;
		ReloadTime = reloadTime;

		CurrentAmmo = MagSize;
		CurrentMaxAmmo = MaxAmmo;
	}

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

		Log.Send($"Reloaded: ({CurrentAmmo}/{CurrentMaxAmmo})");
	}
}

public class PlayerEntity : CharacterEntity
{
	public readonly List<Gun> guns = new() //total hardcoded for now
	{
		new Gun("Sig Sauer", 15, 0f, 15, 60, 1.4f),
		new Gun("AK-47", 30, 0.1f, 30, 120, 2f),
	};

	private LinecastHit laserHit;
	private Light lightSelf;
	private Light flashLight;

	private Light muzzleFlash;
	private bool flashLightOn;

	private int currentGunIndex;
	private Gun currentGun => guns[currentGunIndex];
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

		if (fireTimer > 0)
			fireTimer -= dt;

		if (reloadTimer > 0)
		{
			reloadTimer -= dt;
			if (reloadTimer <= 0)
				currentGun.DoReload();
		}

		if (fireTimer <= 0 && reloadTimer <= 0) //temporary if-else chain for now
		{
			if (InputManager.Weapon1JustPressed)
			{
				currentGunIndex = 0;
				Log.Send($"Switched to: {currentGun.Name}");
			}
			else if (InputManager.Weapon2JustPressed)
			{
				currentGunIndex = 1;
				Log.Send($"Switched to: {currentGun.Name}");
			}
			else if (InputManager.ReloadJustPressed && currentGun.CanReload())
			{
				reloadTimer = currentGun.ReloadTime;
				Log.Send($"Reloading...");
			}
			else if (currentGun.CurrentAmmo > 0)
			{
				if ((currentGun.FiringRate <= 0 && InputManager.ActionJustPressed) || (currentGun.FiringRate > 0 && InputManager.ActionDown))
				{
					if (muzzleFlash != null)
					{
						LightingSystem.RemoveLight(muzzleFlash);
					}

					muzzleFlash = LightingSystem.AddLight(AssetManager.GetSprite("light"), Position, new(80, 30, 0), 0, 14);

					if (laserHit.Body != null && laserHit.Body.SourceEntity is ZombieEntity z)
						z.ApplyDamage(currentGun.Damage);

					currentGun.CurrentAmmo -= 1;
					Log.Send($"Shoot ({currentGun.CurrentAmmo}/{currentGun.CurrentMaxAmmo})");
					if (currentGun.FiringRate > 0 && currentGun.CurrentAmmo > 0)
						fireTimer = currentGun.FiringRate;
				}
			}
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