using Main.Core;
using Main.Effects;
using Main.Helpers;

namespace Main.Gameplay.Entities.Player;

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

	public string AudioWeaponName;

	public Gun(string name, int damage, float firingRate,
			   int magSize, int maxAmmo, float reloadTime, string audioWeaponName)
	{
		Name = name;
		Damage = damage;
		FiringRate = firingRate;
		MagSize = magSize;
		MaxAmmo = maxAmmo;
		ReloadTime = reloadTime;
		AudioWeaponName = audioWeaponName;

		CurrentAmmo = MagSize;
		CurrentMaxAmmo = MaxAmmo / 4;
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

public class PlayerWeapons : IDisposable
{
	public readonly List<Gun> Guns = new() //total hardcoded for now
	{
		new Gun("Sig Sauer", 15, 0f, 15, 60, 1.3f, "handgun"),
		new Gun("AK-47", 30, 0.1f, 30, 120, 1.4f, "rifle"),
	};

	private int currentGunIndex;
	private Gun currentGun => Guns[currentGunIndex];
	private float fireTimer;
	private float reloadTimer;
	private bool isIraqiReload;
	private Light muzzleFlash;

	private PlayerEntity player;
	private GameplayState gameplayState;
	public LinecastHit LaserHit => laserHit;
	private LinecastHit laserHit;

	public PlayerWeapons(GameplayState gameplayState, PlayerEntity player)
	{
		this.player = player;
		this.gameplayState = gameplayState;
	}

	public void Dispose()
	{
		if (muzzleFlash != null)
			LightingSystem.RemoveLight(muzzleFlash);
	}

	public void Update(float dt, float udt)
	{
		gameplayState.GetManager<CollisionManager>().Linecast(player.Position, player.Position + (player.FacingDirection * 100), out laserHit, player.CollisionBody);

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


		if (fireTimer > 0)
			fireTimer -= dt;

		if (reloadTimer > 0)
		{
			reloadTimer -= dt;
			if (reloadTimer <= 0)
			{
				currentGun.DoReload();

				if (!isIraqiReload)
					AudioHandler.PlaySound("weapon/generic/ready");

				isIraqiReload = false;
			}
		}

		if (fireTimer <= 0 && reloadTimer <= 0) //temporary if-else chain for now (FSMs are not needed yet)
		{
			if (InputManager.Weapon1JustPressed)
			{
				currentGunIndex = 0;
				Log.Send($"Switched to: {currentGun.Name}");
				AudioHandler.PlaySound("weapon/generic/equip");
			}
			else if (InputManager.Weapon2JustPressed)
			{
				currentGunIndex = 1;
				Log.Send($"Switched to: {currentGun.Name}");
				AudioHandler.PlaySound("weapon/generic/equip");
			}
			else if (InputManager.ReloadJustPressed && currentGun.CanReload())
			{
				//I just feel like adding Iraqi reload here because it's cheap and cool tbh ("sometimes a cigar is just a cigar" of game design)

				//how it works:
				//if a weapon is an auto and mag is empty, hold the trigger while reloading to make the reload faster
				//IRL equivalent of holding the charging handle ready while loading the new mag
				//this game has no charging handle for guns, so trigger is the closest alternative
				isIraqiReload = currentGun.FiringRate > 0 && currentGun.CurrentAmmo == 0 && InputManager.ActionDown;
				reloadTimer = currentGun.ReloadTime * (isIraqiReload ? 0.6f : 1); //40% is OK enough

				if (isIraqiReload)
					AudioHandler.PlaySound($"weapon/generic/reloadfast");
				else
					AudioHandler.PlaySound($"weapon/{currentGun.AudioWeaponName}/reload");
				Log.Send($"Reloading...");
			}
			else if (
				(currentGun.CurrentAmmo == 0 && InputManager.ActionJustPressed) || //guaranteed tap-to-shoot for dryfire
				(currentGun.CurrentAmmo > 0 && (
					(currentGun.FiringRate <= 0 && InputManager.ActionJustPressed) ||
					(currentGun.FiringRate > 0 && InputManager.ActionDown)
				))
			)
			{
				if (currentGun.CurrentAmmo == 0)
				{
					AudioHandler.PlaySound("weapon/generic/dryfire");
				}
				else
				{
					AudioHandler.PlaySound($"weapon/{currentGun.AudioWeaponName}/fire");

					if (muzzleFlash != null)
					{
						LightingSystem.RemoveLight(muzzleFlash);
					}

					muzzleFlash = LightingSystem.AddLight(AssetManager.GetSprite("light"), player.Position, new(80, 30, 0), 0, 14);

					if (LaserHit.Body != null && LaserHit.Body.SourceEntity is ZombieEntity z)
						z.ApplyDamage(currentGun.Damage);

					currentGun.CurrentAmmo -= 1;
					Log.Send($"Shoot ({currentGun.CurrentAmmo}/{currentGun.CurrentMaxAmmo})");
					if (currentGun.FiringRate > 0 && currentGun.CurrentAmmo > 0)
						fireTimer = currentGun.FiringRate;
				}
			}
		}
	}
}