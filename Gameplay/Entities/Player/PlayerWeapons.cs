using Main.Core;
using Main.Effects;
using Main.Helpers;

namespace Main.Gameplay.Entities.Player;

public class Gun
{
	public string ANIM_IDLE => $"player-{ID}-idle";
	public string ANIM_MOVE => $"player-{ID}-move";
	public string ANIM_SHOOT => $"player-{ID}-shoot";
	public string ANIM_MELEE => $"player-{ID}-meleeattack";
	public string ANIM_RELOAD => $"player-{ID}-reload";

	public string SFX_FIRE => $"weapon/{ID}/fire";
	public string SFX_RELOAD => $"weapon/{ID}/reload";

	public string ID;
	public string Name;
	public int MaxAmmo;
	public int MagSize;
	public float FiringRate; // FiringRate <= 0 means tap to shoot
	public int Damage;
	public int AltDamage;
	public float ReloadTime;

	public int CurrentAmmo;
	public int CurrentMaxAmmo;

	public float NormalizedCurrentAmmoCount => (float)(CurrentAmmo + CurrentMaxAmmo) / (MagSize + MaxAmmo);

	public Gun(string id, string name, int damage, int altDamage, float firingRate,
			   int magSize, int maxAmmo, float reloadTime)
	{
		ID = id;
		Name = name;
		Damage = damage;
		AltDamage = altDamage;
		FiringRate = firingRate;
		MagSize = magSize;
		MaxAmmo = maxAmmo;
		ReloadTime = reloadTime;

		CurrentAmmo = MagSize;
		CurrentMaxAmmo = MaxAmmo / 4;
	}

	public bool CanReload()
	{
		return CurrentAmmo < MagSize && CurrentMaxAmmo > 0;
	}

	public bool DoReload()
	{
		if (!CanReload()) return false;

		int ammoToAdd = Math.Min(MagSize - CurrentAmmo, CurrentMaxAmmo);
		CurrentAmmo += ammoToAdd;
		CurrentMaxAmmo -= ammoToAdd;

		Log.Send($"Reloaded: ({CurrentAmmo}/{CurrentMaxAmmo})");
		return true;
	}
}

public class PlayerWeapons : IDisposable
{
	private const string SFX_DRYFIRE = "weapon/generic/dryfire";
	private const string SFX_READY = "weapon/generic/ready";
	private const string SFX_RELOADFAST = "weapon/generic/reloadfast";
	private const string SFX_EQUIP = "weapon/generic/equip";
	private const string SFX_BULLET_HIT = "weapon/generic/bullethit";
	private const string SFX_MELEE_START = "weapon/generic/meleestart";
	private const string SFX_MELEE_HIT = "weapon/generic/meleehit";

	public readonly List<Gun> Weapons = new() //total hardcoded for now
	{
		new Gun("handgun", "Sig Sauer", 15, 25, 0f, 15, 60, 1.3f),
		new Gun("rifle", "AK-47", 30, 40, 0.1f, 30, 120, 1.4f),
	};

	private int currentWeaponIndex;
	public Gun CurrentWeapon => Weapons[currentWeaponIndex];
	private float fireTimer;
	private Light muzzleFlash;

	private PlayerEntity player;
	private GameplayState gameplayState;
	public LinecastHit LaserHit => laserHit;
	private LinecastHit laserHit;
	private bool isIraqiReload;

	public readonly Signal<Gun> OnWeaponSelected = new();
	public readonly Signal<Gun> OnWeaponAmmoChanged = new();
	public readonly Signal<Gun> OnWeaponFire = new();
	public readonly Signal<(Gun, BaseEntity)> OnWeaponHit = new();
	public readonly Signal<(Gun, BaseEntity)> OnWeaponKill = new();

	public float NormalizedTotalAmmoCount { get; private set; }
	public float Accuracy => fireCount < 3 ? 0 : (float)hitCount / (float)fireCount;

	private int fireCount, hitCount;

	public PlayerWeapons(GameplayState gameplayState, PlayerEntity player)
	{
		this.player = player;
		this.gameplayState = gameplayState;

		foreach (var i in Weapons)
		{
			player.Animator.Add(i.ANIM_IDLE, 0);
			player.Animator.Add(i.ANIM_MOVE, 0);
			player.Animator.Add(i.ANIM_SHOOT, 50);
			player.Animator.Add(i.ANIM_MELEE, 51);
			player.Animator.Add(i.ANIM_RELOAD, 52);
		}

		OnWeaponSelected.Publish(CurrentWeapon);
		UpdateAmmoCount();
	}

	private void UpdateAmmoCount()
	{
		if (Weapons.Count == 0)
		{
			NormalizedTotalAmmoCount = 0;
			return;
		}

		NormalizedTotalAmmoCount = Weapons.Sum(p => p.NormalizedCurrentAmmoCount) / Weapons.Count;
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

		if (player.IsAnimatorBusy)
			return;

		if (fireTimer <= 0)
		{
			if (InputManager.IsPressed(InputAction.AltFire))
			{
				if (player.Animator.Play(CurrentWeapon.ANIM_MELEE))
				{
					fireCount++;
					AudioHandler.PlaySound(SFX_MELEE_START);
				}
			}

			if (InputManager.IsPressed(InputAction.Weapon1))
			{
				currentWeaponIndex = 0;
				OnWeaponSelected.Publish(CurrentWeapon);
				Log.Send($"Switched to: {CurrentWeapon.Name}");
				AudioHandler.PlaySound(SFX_EQUIP);
			}
			else if (InputManager.IsPressed(InputAction.Weapon2))
			{
				currentWeaponIndex = 1;
				OnWeaponSelected.Publish(CurrentWeapon);
				Log.Send($"Switched to: {CurrentWeapon.Name}");
				AudioHandler.PlaySound(SFX_EQUIP);
			}
			else if (InputManager.IsPressed(InputAction.Reload) && CurrentWeapon.CanReload())
			{
				//I just feel like adding Iraqi reload here because it's cheap and cool tbh ("sometimes a cigar is just a cigar" of game design)

				//how it works:
				//if a weapon is an auto and mag is empty, hold the trigger while reloading to make the reload faster
				//IRL equivalent of holding the charging handle ready while loading the new mag
				//this game has no charging handle for guns, so trigger is the closest alternative

				isIraqiReload = CurrentWeapon.FiringRate > 0 && CurrentWeapon.CurrentAmmo == 0 && InputManager.IsDown(InputAction.Fire);
				if (player.Animator.Play(CurrentWeapon.ANIM_RELOAD, targetStartTime: isIraqiReload ? 0.4f : 0f))
				{
					if (isIraqiReload)
						AudioHandler.PlaySound(SFX_RELOADFAST);
					else
						AudioHandler.PlaySound(CurrentWeapon.SFX_RELOAD);

					Log.Send($"Reloading...");
				}
				else
				{
					isIraqiReload = false;
				}
			}
			else if (
				(CurrentWeapon.CurrentAmmo == 0 && InputManager.IsPressed(InputAction.Fire)) || //guaranteed tap-to-shoot for dryfire
				(CurrentWeapon.CurrentAmmo > 0 && (
					(CurrentWeapon.FiringRate <= 0 && InputManager.IsPressed(InputAction.Fire)) ||
					(CurrentWeapon.FiringRate > 0 && InputManager.IsDown(InputAction.Fire))
				))
			)
			{
				if (CurrentWeapon.CurrentAmmo == 0)
				{
					AudioHandler.PlaySound(SFX_DRYFIRE);
				}
				else
				{
					fireCount++;

					OnWeaponFire.Publish(CurrentWeapon);
					player.Animator.Play(CurrentWeapon.ANIM_SHOOT);
					AudioHandler.PlaySound(CurrentWeapon.SFX_FIRE);

					if (muzzleFlash != null)
					{
						LightingSystem.RemoveLight(muzzleFlash);
					}

					muzzleFlash = LightingSystem.AddLight("light", player.Position, new(80, 30, 0), 0, 14);

					if (LaserHit.Body != null && LaserHit.Body.SourceEntity is ZombieEntity z)
					{
						AudioHandler.PlaySound(SFX_BULLET_HIT, z.Position);

						hitCount++;
						OnWeaponHit.Publish((CurrentWeapon, z));
						z.ApplyDamage(CurrentWeapon.Damage);

						if (z.IsDead)
							OnWeaponKill.Publish((CurrentWeapon, z));
					}

					CurrentWeapon.CurrentAmmo -= 1;
					OnWeaponAmmoChanged.Publish(CurrentWeapon);
					UpdateAmmoCount();
					Log.Send($"Shoot ({CurrentWeapon.CurrentAmmo}/{CurrentWeapon.CurrentMaxAmmo})");
					if (CurrentWeapon.FiringRate > 0 && CurrentWeapon.CurrentAmmo > 0)
						fireTimer = CurrentWeapon.FiringRate;
				}
			}
		}
	}

	public void OnFrameChanged((string, int, float) frameData)
	{
		if (frameData.Item1 != CurrentWeapon.ANIM_MELEE)
			return;

		if (frameData.Item2 != 3)
			return;

		var hit = false;
		foreach (var i in gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(ZombieEntity)))
		{
			var z = i as ZombieEntity;
			if (z.IsDead)
				continue;

			var d = i.Position - player.Position;
			if (!player.FacingDirection.IsInFront(d, 5, 70))
				continue;

			z.ApplyDamage(CurrentWeapon.AltDamage);
			hit = true;
		}

		if (hit)
			AudioHandler.PlaySound(SFX_MELEE_HIT);
	}

	internal void OnAnimationEnd(string animationName)
	{
		if (animationName == CurrentWeapon.ANIM_RELOAD)
		{
			if (CurrentWeapon.DoReload())
			{
				if (!isIraqiReload)
					AudioHandler.PlaySound(SFX_READY);

				OnWeaponAmmoChanged.Publish(CurrentWeapon);
				UpdateAmmoCount();
			}

			isIraqiReload = false;
		}
	}
}