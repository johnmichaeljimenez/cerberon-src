using Main.Core;
using Main.Gameplay.Entities;
using Main.Gameplay.Entities.Player;
using Main.Helpers;
using Tween;

namespace Main.Gameplay.Managers;

//TODO: make all magic numbers editable outside
//TODO: track the player's most frequently visited locations and make items spawn there
public class AIDirectorManager : BaseManager
{
	private readonly List<string> BGMList = new(){
		"bone shredder",
		"carnivore diet",
		"cerebral mutilation"
	};
	private int bgmIndex;

	[DataConfig(defaultValue: true)]
	public static bool Enabled;

	//TENSION is for gameplay difficulty (spawn rates, aggression, events)
	public enum TensionState { Calm, Tense, Panic, Critical }
	//MOOD is for atmosphere and emotion (music, colors, effects)
	public enum MoodState { Calm, Anxious, Dread, Terror }

	private float emaTensionLock;
	private float emaMoodLock;

	public readonly HysteresisState<TensionState> CurrentTensionState = new(new List<(TensionState, float)>()
		{
			(TensionState.Calm, 0f),
			(TensionState.Tense, 0.2f),
			(TensionState.Panic, 0.5f),
			(TensionState.Critical, 0.7f),
		}
	);

	public readonly HysteresisState<MoodState> CurrentMood = new(new List<(MoodState, float)>()
		{
			(MoodState.Calm, 0f),
			(MoodState.Anxious, 0.2f),
			(MoodState.Dread, 0.65f),
			(MoodState.Terror, 0.85f),
		}
	);

	private readonly EMA emaPlayerHealth = new(0.05f);
	private readonly EMA emaAmmoCount = new(0.05f);
	private readonly EMA emaPlayerAccuracy = new(0.005f);
	private readonly EMA emaKillCount = new(0.003f);
	private readonly EMA emaPlayerHurt = new(0.003f);
	private readonly EMA emaNearbyEnemyCount = new(0.003f);

	private readonly EMA emaTension = new(0.005f);
	private readonly EMA emaMood = new(0.005f);

	public float Tension { get; private set; }
	public float Mood { get; private set; }

	private const int MAX_ENEMY_COUNT = 50;
	private const int MAX_ITEM_HEALTH_COUNT = 3;
	private const int MAX_ITEM_AMMO_COUNT = 5;
	private const int MAX_ITEM_WEAPON_COUNT = 1;

	private float enemySpawnTimer;
	private float healthSpawnTimer;
	private float ammoSpawnTimer;
	private float weaponSpawnTimer;

	private PlayerEntity player;
	private GameplayManager gameplayManager;

	public bool Paused = false;

	//this enemy attack token system prevents the player from get shredded quickly by horde of enemies
	private const int MAX_ATTACKING_ENEMY = 2;
	private const float TOKEN_COOLDOWN = 0.5f;
	private readonly float[] attackTokenCooldowns = new float[MAX_ATTACKING_ENEMY];

	public AIDirectorManager(GameplayState gameplayState) : base(gameplayState)
	{
		enemySpawnTimer = 1f;
		healthSpawnTimer = 0f;
		ammoSpawnTimer = 0f;
		weaponSpawnTimer = 30f;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BGMList.Shuffle();
		bgmIndex = 0;

		gameplayManager = gameplayState.GetManager<GameplayManager>();
		player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;

		gameplayState.CurrentWorld.OnEntityDespawn.Subscribe(e =>
		{
			if (e is EnemyEntity z)
			{
				if (z.HP <= 0)
				{
					emaKillCount.AddSample(60.0f); //large bump to compensate for decay (add must be faster than reduction)
				}
			}
		}).AddTo(disposables);

		player.OnTakeDamage.Subscribe(dmg =>
		{
			emaPlayerHurt.AddSample(dmg * 40);
			emaMood.AddSample(emaMood.Current + (dmg * 0.5f));
		}).AddTo(disposables);
	}

	public void Begin()
	{
		Paused = false;
		if (Enabled)
			OnMoodChanged();
	}

	public override void Update(float dt, float udt)
	{
		if (!Enabled || PauseHandler.IsPaused || Paused) return;

		base.Update(dt, udt);

		for (int i = 0; i < attackTokenCooldowns.Length; i++)
		{
			Utils.Countdown(ref attackTokenCooldowns[i], dt);
		}

		TensionUpdate(dt);
		MoodUpdate(dt);

		if (emaPlayerHealth.Current <= 0) return;

		enemySpawnTimer -= dt;
		if (enemySpawnTimer <= 0)
		{
			enemySpawnTimer = CalculateEnemySpawnInterval();
			int current = gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(EnemyEntity)).Count;
			if (current < MAX_ENEMY_COUNT)
			{
				int toSpawn = CalculateEnemysToSpawn();

				for (int i = 0; i < toSpawn; i++) SpawnEnemy();
			}
		}

		healthSpawnTimer -= dt;
		if (healthSpawnTimer <= 0 && (emaPlayerHealth.Current < 0.8f || emaPlayerHurt.Current >= 0.5f)) //allow health spawn when player takes too much damage in short time
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("health").Count < MAX_ITEM_HEALTH_COUNT)
			{
				SpawnHealthItem();
				healthSpawnTimer = 10f;
			}
		}

		ammoSpawnTimer -= dt;
		if (ammoSpawnTimer <= 0 && emaAmmoCount.Current < 0.7f)
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("ammo").Count < MAX_ITEM_AMMO_COUNT)
			{
				SpawnAmmoItem();
				ammoSpawnTimer = 15f;
			}
		}

		if (weaponSpawnTimer > 0)
		{
			weaponSpawnTimer -= dt;
			if (weaponSpawnTimer <= 0)
			{
				weaponSpawnTimer = 5.0f;
				if (gameplayManager.NormalizedTime <= 0.25f || emaTension.Current >= 0.6f)
				{
					var allWeaponsUnlocked = player.Weapons.Weapons.All(p => p.IsUnlocked);
					if (allWeaponsUnlocked)
					{
						weaponSpawnTimer = -1;
					}
					else
					{
						var spawned = false;

						if (gameplayState.CurrentWorld.GetEntitiesByGroup("weapon").Count < MAX_ITEM_WEAPON_COUNT)
						{
							if (!player.Weapons.IsWeaponUnlocked("rifle"))
							{
								SpawnWeaponItem("rifle");
								spawned = true;
							}
							else if (!player.Weapons.IsWeaponUnlocked("shotgun"))
							{
								SpawnWeaponItem("shotgun");
								spawned = true;
							}
						}

						weaponSpawnTimer = spawned ? 20f : 5f;
					}
				}
			}
		}
	}

	private void TensionUpdate(float dt)
	{
		if (emaTensionLock > 0)
			return;

		emaKillCount.AddSample(0f);
		emaPlayerHurt.AddSample(0f);

		emaPlayerAccuracy.AddSample(player.Weapons.Accuracy);
		emaPlayerHealth.AddSample((float)player.HP / player.MaxHP);
		emaAmmoCount.AddSample(player.Weapons.NormalizedTotalAmmoCount);

		var zList = gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(EnemyEntity));
		var nearbyEnemyCount = 0;
		for (int i = 0; i < zList.Count; i++)
		{
			var z = zList[i] as EnemyEntity;
			if (z.IsDestroyed || z.IsDead)
				continue;

			if ((z.Position - player.Position).LengthSquared() <= 10 * 10)
				nearbyEnemyCount++;
		}

		emaNearbyEnemyCount.AddSample(((float)nearbyEnemyCount / 5) * 2);

		var newTension = emaKillCount.Current * 0.4f +
						 emaPlayerAccuracy.Current * 0.1f +
						 emaPlayerHealth.Current * 0.5f;

		newTension -= emaPlayerHurt.Current * 0.5f;

		newTension = Math.Clamp(newTension, 0f, 1f);
		emaTension.AddSample(newTension);

		if (emaTensionLock <= 0)
			Tension = Raymath.Lerp(Tension, emaTension.Current, 5f * dt);

		if (CurrentTensionState.Update(Tension))
		{
			TweenManager.Add(new Tween<float>(() => emaTensionLock, p => emaTensionLock = p, 0, 10f, 1, "TensionLock", false).SetEasing(Easing.QuadIn));
		}
	}

	private void MoodUpdate(float dt)
	{
		if (emaMoodLock > 0)
			return;

		float moodInput = (1.0f - emaPlayerHealth.Current) * 0.45f + //low health = more intense mood
						  emaNearbyEnemyCount.Current * 0.5f +
						  emaKillCount.Current * 0.3f +
						  emaAmmoCount.Current * 0.1f;

		moodInput = Math.Clamp(moodInput, 0f, 1f);
		emaMood.AddSample(moodInput);

		Mood = emaMood.Current;

		if (CurrentMood.Update(Mood))
		{
			TweenManager.Add(new Tween<float>(() => emaMoodLock, p => emaMoodLock = p, 0, 10f, 1, "MoodLock", false).SetEasing(Easing.QuadIn));
			OnMoodChanged();
		}
	}

	private float CalculateEnemySpawnInterval() => 9.5f - Tension * 7.8f;
	private int CalculateEnemysToSpawn() =>
		CurrentTensionState.CurrentState switch
		{
			TensionState.Calm => 1,
			TensionState.Tense => 2,
			TensionState.Panic => 3,
			TensionState.Critical => 5,
			_ => 2
		};

	private Vector2 GetSpawnPosition() =>
		gameplayState.GetManager<WaypointManager>().GetNodePosition(
			player.Position, 28f, 36f
		);

	private void SpawnHealthItem() => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = ItemPickupEntity.ItemTypes.Health;
		e.Amount = 30;
		e.Position = GetSpawnPosition();
	});

	private void SpawnAmmoItem() => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = ItemPickupEntity.ItemTypes.Ammo;
		e.Position = GetSpawnPosition();
	});

	private void SpawnWeaponItem(string id) => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = id == "shotgun" ? ItemPickupEntity.ItemTypes.WeaponShotgun : ItemPickupEntity.ItemTypes.WeaponAK;
		e.Position = GetSpawnPosition();
	});

	private void SpawnEnemy() => gameplayState.CurrentWorld.SpawnEntity<EnemyEntity>(e =>
	{
		e.Position = GetSpawnPosition() + RNG.Position(0.2f);
		e.IsFlyer = CurrentTensionState.CurrentState >= TensionState.Critical && RNG.Chance(0.5f); //jumpscare + escalate pressure (but only if player can sustain it)
	});

	public override void DrawImGui()
	{
		base.DrawImGui();
		ImGui.Checkbox("Pause", ref Paused);
		ImGui.SeparatorText($"AI Director: (Tension: {CurrentTensionState.CurrentState}, Mood: {CurrentMood.CurrentState})");
		ImGui.Text($"Tension Lock: {emaTensionLock:F2}");
		ImGui.Text($"Mood Lock: {emaMoodLock:F2}");
		ImGui.ProgressBar(Tension, new(340, 25), $"Overall Tension: {emaTension.Current:F2}");
		ImGui.ProgressBar(Mood, new(340, 25), $"Mood: {emaMood.Current:F2}");

		ImGui.SeparatorText("Data");
		ImGui.ProgressBar(emaKillCount.Current, new(340, 25), $"Kill Rate: {emaKillCount.Current:F2}");
		ImGui.ProgressBar(emaPlayerAccuracy.Current, new(340, 25), $"Accuracy: {emaPlayerAccuracy.Current:F2}");
		ImGui.ProgressBar(emaPlayerHealth.Current, new(340, 25), $"Health: {emaPlayerHealth.Current:F2}");
		ImGui.ProgressBar(emaAmmoCount.Current, new(340, 25), $"Ammo: {emaAmmoCount.Current:F2}");
		ImGui.ProgressBar(emaPlayerHurt.Current, new(340, 25), $"Damage Taken: {emaPlayerHurt.Current:F2}");
		ImGui.ProgressBar(emaNearbyEnemyCount.Current, new(340, 25), $"Nearby enemy Count: {emaNearbyEnemyCount.Current:F2}");

		ImGui.SeparatorText("Attack tokens");
		for (int i = 0; i < attackTokenCooldowns.Length; i++)
		{
			var val = attackTokenCooldowns[i] / TOKEN_COOLDOWN;
			ImGui.ProgressBar(val, new(340, 20), $"{val:F2} s");
		}
	}

	public int RequestAttack()
	{
		for (int i = 0; i < MAX_ATTACKING_ENEMY; i++)
		{
			if (attackTokenCooldowns[i] <= 0)
			{
				attackTokenCooldowns[i] = float.MaxValue;
				return i;
			}
		}
		return -1; //cannot attack yet
	}

	public void ReleaseAttack(int tokenIndex)
	{
		if (tokenIndex >= 0 && tokenIndex < MAX_ATTACKING_ENEMY)
		{
			//give this token a cooldown before being able to be reused
			attackTokenCooldowns[tokenIndex] = RNG.Range(TOKEN_COOLDOWN * 0.8f, TOKEN_COOLDOWN * 2);
		}
	}

	private void OnMoodChanged()
	{
		AudioHandler.PlayMusic(BGMList[bgmIndex], CurrentMood.CurrentState.ToString().ToLower());

		bgmIndex++;
		if (bgmIndex >= BGMList.Count)
		{
			BGMList.Shuffle();
			bgmIndex = 0;
		}
	}
}