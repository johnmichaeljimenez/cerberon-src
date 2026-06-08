using Cerberon.Core;
using Cerberon.Gameplay.Entities;
using Cerberon.Gameplay.Entities.Player;
using Cerberon.Helpers;
using Tween;

namespace Cerberon.Gameplay.Managers;


//TODO: make all magic numbers editable outside
//TODO: track the player's most frequently visited locations and make items spawn there
public class AIDirectorManager : BaseManager
{
	[DataConfig(15)]
	public static float SpawnDistanceMin;
	[DataConfig(50)]
	public static float SpawnDistanceMax;

	[DataConfig]
	private static List<string> BGMList = new(){
		"bone shredder",
		"carnivore diet",
		"cerebral mutilation"
	};

	private int bgmIndex;

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

	[DataConfig]
	private static EMA emaPlayerHealth = new(0.05f);
	[DataConfig]
	private static EMA emaAmmoCount = new(0.05f);
	[DataConfig]
	private static EMA emaPlayerAccuracy = new(0.005f);
	[DataConfig]
	private static EMA emaKillCount = new(0.003f);
	[DataConfig]
	private static EMA emaWeaponUseType = new(0.001f);
	[DataConfig]
	private static EMA emaPlayerHurt = new(0.003f);
	[DataConfig]
	private static EMA emaNearbyEnemyCount = new(0.003f);

	[DataConfig]
	private static EMA emaTension = new(0.005f);
	[DataConfig]
	private static EMA emaMood = new(0.005f);

	public float Tension { get; private set; }
	public float Mood { get; private set; }

	private float killType;

	[DataConfig]
	private static int MaxItemHealthCount = 3;
	[DataConfig]
	private static int MaxItemAmmoCount = 5;
	[DataConfig]
	private static int MaxItemWeaponCount = 1;

	private float enemySpawnTimer;
	private float healthSpawnTimer;
	private float ammoSpawnTimer;
	private float weaponSpawnTimer;

	private PlayerEntity player;
	private GameplayManager gameplayManager;

	public bool Paused = true;

	//this enemy attack token system prevents the player from get shredded quickly by horde of enemies
	[DataConfig]
	private static int MaxAttackingEnemy = 2;
	private const float TOKEN_COOLDOWN = 0.5f;
	private const float KillCountBump = 60.0f;
	private const int HurtPlayerBump = 20;
	private const int WeaponHitBump = 30;
	private const int CampingDecayScale = 4;
	private const float CampingDecayThreshold = 1.5f;
	private const int CampingRateMultiplier = 5;
	private const int NearbyEnemyMaxDistance = 10;
	private readonly float[] attackTokenCooldowns = new float[MaxAttackingEnemy];

	private Vector2 playerPreviousPosition;
	[DataConfig]
	private static EMA emaPlayerPosition = new(0.01f);
	[DataConfig]
	private static EMA emaMovementRate = new(0.01f);

	private float currentBudget;

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

		//TODO: play a count-in hihat sfx here before playing bgm
		BGMList.Shuffle();
		bgmIndex = 0;

		gameplayManager = gameplayState.GetManager<GameplayManager>();
		player = gameplayState.GetManager<GameplayManager>().PlayerCharacter;

		gameplayManager.OnFightStart.Subscribe(e =>
		{
			playerPreviousPosition = player.Position;
			Paused = false;
			OnMoodChanged();
		});

		gameplayState.CurrentWorld.OnEntityDespawn.Subscribe(e =>
		{
			if (e is EnemyEntity z)
			{
				if (z.HP <= 0)
				{
					emaKillCount.AddSample(KillCountBump);
				}
			}
		}).AddTo(disposables);

		player.OnTakeDamage.Subscribe(dmg =>
		{
			emaPlayerHurt.AddSample(dmg * HurtPlayerBump);
			emaMood.AddSample(emaMood.Current + (dmg * 0.5f));
		}).AddTo(disposables);

		player.Weapons.OnWeaponHit.Subscribe(e =>
		{
			var amt = -1f;
			if (e.Item3 || e.Item1.ID == "shotgun") //is melee or shotgun
				amt = 1f;

			emaWeaponUseType.AddSample(amt * WeaponHitBump);
		});

		playerPreviousPosition = player.Position;
	}

	public override void Update(float dt, float udt)
	{
		if (PauseHandler.IsPaused)
			return;

		var prevSample = emaPlayerPosition.Current;
		playerPreviousPosition = Vector2.Lerp(playerPreviousPosition, player.Position, dt * CampingDecayScale);
		var diff = player.Position - playerPreviousPosition;
		var magnitude = diff.Length();
		var scale = 0f;

		if (magnitude < CampingDecayThreshold)
		{
			diff = Vector2.Zero;
		}
		else
		{
			scale = (magnitude - CampingDecayThreshold) / magnitude;
			diff = diff * scale;
		}

		emaPlayerPosition.AddSample(scale);
		emaMovementRate.AddSample(prevSample * CampingRateMultiplier);  //used to check if player moves or camps a lot

		if (Paused) return;

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
			var maxCost = CalculateEnemiesToSpawn();    //TODO: improve

			var cost = Utils.Shatter(ref maxCost, 0.5f, Raymath.Remap((float)CurrentTensionState.CurrentState, 0f, 3f, 0.5f, 1f));
			foreach (var i in cost)
			{
				SpawnEnemy(i);
			}
		}

		healthSpawnTimer -= dt;
		if (healthSpawnTimer <= 0 && (emaPlayerHealth.Current < 0.8f || emaPlayerHurt.Current >= 0.5f)) //allow health spawn when player takes too much damage in short time
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("health").Count < MaxItemHealthCount)
			{
				SpawnHealthItem();
				healthSpawnTimer = 10f;
			}
		}

		ammoSpawnTimer -= dt;
		if (ammoSpawnTimer <= 0 && emaAmmoCount.Current < 0.7f)
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("ammo").Count < MaxItemAmmoCount)
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
				if (gameplayManager.NormalizedTime <= 0.1f || emaTension.Current >= 0.4f)
				{
					var allWeaponsUnlocked = player.Weapons.Weapons.All(p => p.IsUnlocked);
					if (allWeaponsUnlocked)
					{
						weaponSpawnTimer = -1;
					}
					else
					{
						var spawned = false;

						if (gameplayState.CurrentWorld.GetEntitiesByGroup("weapon").Count < MaxItemWeaponCount)
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
		emaWeaponUseType.AddSample(0f); //no need to lock
		killType = Raymath.Clamp(Raymath.Remap(emaWeaponUseType.Current, -1f, 1f, 0.5f, 2f), 0.5f, 2f);

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

			if ((z.Position - player.Position).LengthSquared() <= NearbyEnemyMaxDistance * NearbyEnemyMaxDistance)
				nearbyEnemyCount++;
		}

		emaNearbyEnemyCount.AddSample(((float)nearbyEnemyCount / 5) * 2);

		var newTension = emaKillCount.Current * 0.8f +
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
			//TODO: get the difference between previous and current (changed) mood.
			//if it rises sharply, play a guitar pick slide/grind sfx before switching music
			//if it drops sharply, play a guitar feedback fading out tone sfx before switching music

			TweenManager.Add(new Tween<float>(() => emaMoodLock, p => emaMoodLock = p, 0, 10f, 1, "MoodLock", false).SetEasing(Easing.QuadIn));
			OnMoodChanged();
		}
	}

	private float CalculateEnemySpawnInterval() => Raymath.Remap(Raymath.Clamp01(Tension), 0f, 1f, 3f, 7f);
	private float CalculateEnemiesToSpawn()
	{
		if (gameplayManager.NormalizedTime >= 1.0f)
			return 0;

		return CurrentTensionState.CurrentState switch
		{
			TensionState.Calm => 1,
			TensionState.Tense => 2,
			TensionState.Panic => 3,
			TensionState.Critical => 5,
			_ => 2
		};
	}

	private Vector2 GetSpawnPosition(bool hidden)
	{
		var l = gameplayState.GetManager<CollisionManager>();
		Func<Vector2, Vector2, bool> func = (from, to) =>
		{
			return l.Linecast(from, to, CollisionHeight.Mid, out var info, null, true);
		};

		var pos = Vector2.Zero;
		if (hidden)
			pos = gameplayState.CurrentWorld.NodeData.GetExposedNode(player.Position, SpawnDistanceMin, SpawnDistanceMax, func).Position;
		else
			pos = gameplayState.CurrentWorld.NodeData.GetHiddenNode(player.Position, SpawnDistanceMin, SpawnDistanceMax, func).Position;

		return pos + RNG.Position(0.4f);
	}

	private void SpawnHealthItem() => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = ItemPickupEntity.ItemTypes.Health;
		e.Amount = 30;
		e.Position = GetSpawnPosition(false);
	});

	private void SpawnAmmoItem() => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = ItemPickupEntity.ItemTypes.Ammo;
		e.Position = GetSpawnPosition(false);
	});

	private void SpawnWeaponItem(string id) => gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
	{
		e.ItemType = id == "shotgun" ? ItemPickupEntity.ItemTypes.WeaponShotgun : ItemPickupEntity.ItemTypes.WeaponAK;
		e.Position = GetSpawnPosition(false);
	});

	private void SpawnEnemy(float cost) => gameplayState.CurrentWorld.SpawnEntity<EnemyEntity>(e =>
	{
		e.Position = GetSpawnPosition(true);

		//make enemies fly if player camps too much (but only if player can sustain the pressure)
		e.IsFlyer = CurrentTensionState.CurrentState >= TensionState.Panic && RNG.Chance(0.5f) && emaMovementRate.Current < 0.1f;
		e.Cost = cost;
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
		ImGui.ProgressBar(emaMovementRate.Current, new(340, 25), $"Player movement rate: {emaMovementRate.Current:F2}");

		var kt = Raymath.Remap(killType, 0.1f, 2f, 0f, 1f);
		ImGui.ProgressBar(kt, new(340, 25), $"Kill Type: {kt:F2}");

		ImGui.SeparatorText("Attack tokens");
		for (int i = 0; i < attackTokenCooldowns.Length; i++)
		{
			var val = attackTokenCooldowns[i] / TOKEN_COOLDOWN;
			ImGui.ProgressBar(val, new(340, 20), $"{val:F2} s");
		}
	}

	public int RequestAttack()
	{
		for (int i = 0; i < MaxAttackingEnemy; i++)
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
		if (tokenIndex >= 0 && tokenIndex < MaxAttackingEnemy)
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