using Main.Core;
using Main.Effects;
using Main.Gameplay.Managers;
using Main.Helpers;
using Tween;

namespace Main.Gameplay.Entities.Player;

public class Weapon
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
	public int SpreadCount;
	public float SpreadAngle;
	public float FiringRate; // FiringRate <= 0 means tap to shoot
	public int Damage;
	public int AltDamage;

	//knockback
	public float MeleeKick;
	public float RangedKick;

	public int CurrentAmmo;
	public int CurrentMaxAmmo;

	public bool UsesAmmo => MaxAmmo + MagSize > 0;
	public float NormalizedCurrentAmmoCount => !UsesAmmo ? 0 : (float)(CurrentAmmo + CurrentMaxAmmo) / (MagSize + MaxAmmo);

	public bool IsUnlocked { get; private set; }

	public Weapon(string id, string name, int damage, int altDamage, float firingRate,
			   int magSize, int maxAmmo, bool unlocked = false, float meleeKick = 60, float rangedKick = 20)
	{
		ID = id;
		Name = name;
		Damage = damage;
		AltDamage = altDamage;
		FiringRate = firingRate;
		MagSize = magSize;
		MaxAmmo = maxAmmo;
		MeleeKick = meleeKick;
		RangedKick = rangedKick;

		IsUnlocked = unlocked;
		GiveAmmo();
	}

	public void GiveAmmo(int ammo1 = -1, int ammo2 = -1)
	{
		if (!IsUnlocked || !UsesAmmo)
			return;

		CurrentAmmo += ammo1 < 0 ? MagSize : ammo1;
		CurrentAmmo = Math.Min(CurrentAmmo, MagSize);
		CurrentMaxAmmo += ammo2 < 0 ? MaxAmmo / 4 : ammo2;
	}

	public void Unlock()
	{
		IsUnlocked = true;
	}

	public bool CanReload()
	{
		return UsesAmmo && CurrentAmmo < MagSize && CurrentMaxAmmo > 0;
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
	private const float MAX_KICK = 80f;
	public readonly List<Weapon> Weapons = new() //total hardcoded for now
	{
		new Weapon("knife", "Knife", 0, 70, 0f, 0, 0, true),
		new Weapon("handgun", "Sig Sauer", 50, 50, 0f, 15, 60, true),
		new Weapon("rifle", "AK-47", 80, 60, 0.1f, 30, 120),
		new Weapon("shotgun", "Sawn-off Shotgun", 70, 60, 0f, 2, 30, false, rangedKick: 80){
			SpreadAngle = 60,
			SpreadCount = 20
		} //2-shot
	};

	private int currentWeaponIndex;
	public Weapon CurrentWeapon => Weapons[currentWeaponIndex];
	private float fireTimer;
	private Light muzzleFlash;

	private PlayerEntity player;
	private GameplayState gameplayState;
	private LinecastHit weaponHit;
	private bool isIraqiReload;

	private LinecastHit spreadHit;

	public readonly Signal<Weapon> OnWeaponSelected = new();
	public readonly Signal<Weapon> OnWeaponAmmoChanged = new();
	public readonly Signal<Weapon> OnWeaponFire = new();
	public readonly Signal<(Weapon, BaseEntity, bool)> OnWeaponHit = new();
	public readonly Signal<(Weapon, BaseEntity, bool)> OnWeaponKill = new();

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
			player.Animator.Add(i.ANIM_MELEE, 50);
			player.Animator.Add(i.ANIM_RELOAD, 50);
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

		// if (player.IsAnimatorBusy)
		// 	return;

		if (fireTimer <= 0)
		{
			if (InputManager.IsPressed(InputAction.AltFire) && CurrentWeapon.UsesAmmo)
			{
				if (player.Animator.Play(CurrentWeapon.ANIM_MELEE))
				{
					fireCount++;
					AudioHandler.PlaySound(SFX_MELEE_START);
				}
			}

			for (int i = 0; i < InputManager.WeaponInputs.Count; i++)
			{
				var input = InputManager.WeaponInputs[i];
				if (InputManager.IsPressed(input) && currentWeaponIndex != i && Weapons[i].IsUnlocked)
				{
					SwitchWeapon(i);
					break;
				}
			}

			if (InputManager.IsPressed(InputAction.Reload) && CurrentWeapon.CanReload())
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
				((CurrentWeapon.CurrentAmmo == 0 || !CurrentWeapon.UsesAmmo) && InputManager.IsPressed(InputAction.Fire)) || //guaranteed tap-to-shoot for dryfire and melee-only
				(CurrentWeapon.CurrentAmmo > 0 && (
					(CurrentWeapon.FiringRate <= 0 && InputManager.IsPressed(InputAction.Fire)) ||
					(CurrentWeapon.FiringRate > 0 && InputManager.IsDown(InputAction.Fire))
				))
			)
			{
				if (!CurrentWeapon.UsesAmmo)
				{
					if (player.Animator.Play(CurrentWeapon.ANIM_MELEE))
					{
						fireCount++;
						OnWeaponFire.Publish(CurrentWeapon);
						AudioHandler.PlaySound(SFX_MELEE_START);
					}
				}
				else
				{
					if (CurrentWeapon.CurrentAmmo == 0)
					{
						AudioHandler.PlaySound(SFX_DRYFIRE);
					}
					else
					{
						fireCount++;

						Game.Instance.Camera.Shake(CurrentWeapon.RangedKick / MAX_KICK, player.FacingDirection); //forward shake relative to player
						OnWeaponFire.Publish(CurrentWeapon);
						player.Animator.Play(CurrentWeapon.ANIM_SHOOT);
						AudioHandler.PlaySound(CurrentWeapon.SFX_FIRE);

						if (muzzleFlash != null)
						{
							LightingSystem.RemoveLight(muzzleFlash);
						}

						muzzleFlash = LightingSystem.AddLight("light", player.Position, new(80, 30, 0), 0, 14);

						if (CurrentWeapon.SpreadCount == 0)
						{
							gameplayState.GetManager<CollisionManager>().Linecast(player.Position, player.Position + (player.FacingDirection * 100), CollisionHeight.Mid, out weaponHit, player.CollisionBody);
							if (weaponHit.Body != null && weaponHit.Body.SourceEntity is EnemyEntity z)
							{
								hitCount++;
								HitBullet(z);
							}
						}
						else
						{
							var hit = false;
							var sc = 0;
							for (int i = 0; i < CurrentWeapon.SpreadCount; i++)
							{
								var half = CurrentWeapon.SpreadAngle / 2;
								var a = Raymath.LerpAngle(-half, half, (float)i / CurrentWeapon.SpreadCount);
								var d = player.GetFacingAngleOffset(a);

								gameplayState.GetManager<CollisionManager>().Linecast(player.Position, player.Position + (d * 100), CollisionHeight.Mid, out spreadHit, player.CollisionBody);
								if (spreadHit.Body != null && spreadHit.Body.SourceEntity is EnemyEntity z)
								{
									if (!hit)
									{
										hit = true;
										hitCount++;
									}

									sc++;
									HitBullet(z);
								}
							}

							Log.Send($"Spread hit: {sc}");

							spreadHit.Body = null;
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
	}

	private void SwitchWeapon(int i)
	{
		player.Animator.Stop(); //bypass hierarchy to switch animations immediately
		currentWeaponIndex = i;
		OnWeaponSelected.Publish(CurrentWeapon);
		Log.Send($"Switched to: {CurrentWeapon.Name}");
		AudioHandler.PlaySound(SFX_EQUIP);
	}

	private void HitBullet(EnemyEntity z)
	{
		AudioHandler.PlaySound(SFX_BULLET_HIT, z.Position);
		OnWeaponHit.Publish((CurrentWeapon, z, false));
		ApplyKick(z, Raymath.Vector2Normalize(z.Position - player.Position), CurrentWeapon.RangedKick);
		z.ApplyDamage(CurrentWeapon.Damage, player);

		if (z.IsDead)
			OnWeaponKill.Publish((CurrentWeapon, z, false));
	}

	public void OnAnimationBegin(string animationName)
	{
		player.Origin = CurrentWeapon.ID == "knife" ? new(0.3f, 0.45f) : new(0.3f, 0.7f);

		if (animationName == CurrentWeapon.ANIM_MELEE)
			Game.Instance.Camera.Shake(CurrentWeapon.MeleeKick / MAX_KICK, player.GetFacingAngleOffset(90)); //side-by-side shake relative to player
	}

	public void OnFrameChanged((string, int, float) frameData)
	{
		if (frameData.Item1 != CurrentWeapon.ANIM_MELEE)
			return;

		if (frameData.Item2 != 3)
			return;

		var hit = false;
		foreach (var i in gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(EnemyEntity)))
		{
			var z = i as EnemyEntity;
			if (z.IsDead)
				continue;

			var d = i.Position - player.Position;
			if (!player.FacingDirection.IsInFront(d, 6, 90))
				continue;

			OnWeaponHit.Publish((CurrentWeapon, z, true));
			ApplyKick(z, Raymath.Vector2Normalize(d), CurrentWeapon.MeleeKick);
			z.ApplyDamage(CurrentWeapon.AltDamage, player);
			hit = true;

			if (z.IsDead)
				OnWeaponKill.Publish((CurrentWeapon, z, true));
		}

		if (hit)
		{
			PauseHandler.ApplyHitstop();
			Game.Instance.Camera.Shake(0.4f, player.GetFacingAngleOffset(90));
			hitCount++;
			AudioHandler.PlaySound(SFX_MELEE_HIT);
		}
	}

	public bool PickupWeapon(string id)
	{
		var w = Weapons.FirstOrDefault(p => p.ID == id);
		if (w == null)
			return false;

		if (!w.IsUnlocked)
		{
			w.Unlock();
			SwitchWeapon(Weapons.IndexOf(w));
		}

		w.GiveAmmo();
		if (w == CurrentWeapon)
			OnWeaponAmmoChanged.Publish(w);

		return true;
	}

	public void OnAnimationEnd(string animationName)
	{
		if (animationName == CurrentWeapon.ANIM_RELOAD)
		{
			if (player.Animator.NormalizedTime >= 1.0f)
			{
				if (CurrentWeapon.DoReload())
				{
					if (!isIraqiReload)
						AudioHandler.PlaySound(SFX_READY);

					OnWeaponAmmoChanged.Publish(CurrentWeapon);
					UpdateAmmoCount();
				}
			}
			else
			{
				Log.Send("Reload cancelled!");
			}

			isIraqiReload = false;
		}
	}

	public bool IsWeaponUnlocked(string id)
	{
		var w = Weapons.FirstOrDefault(p => p.ID == id);
		if (w == null)
			return false;

		return w.IsUnlocked;
	}

	public bool PickupAmmo()
	{
		foreach (var i in Weapons)
		{
			if (i.IsUnlocked)
			{
				i.GiveAmmo(0);  //give reserve ammo
				if (i == CurrentWeapon)
					OnWeaponAmmoChanged.Publish(CurrentWeapon);
			}
		}

		return true;
	}

	private void ApplyKick(CharacterEntity e, Vector2 dir, float amt)
	{
		e.SetExternalForce(dir, amt, 0.1f);
	}
}