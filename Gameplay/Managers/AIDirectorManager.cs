using Main.Core;
using Main.Gameplay.Entities;
using Main.Helpers;

namespace Main.Gameplay.Managers;

//TODO: make all magic numbers editable outside
public class AIDirectorManager : BaseManager
{
	public enum TensionState { Calm, Tense, Panic, Critical }

	public TensionState CurrentState =>
		Tension switch
		{
			< 0.25f => TensionState.Calm,
			< 0.5f => TensionState.Tense,
			< 0.9f => TensionState.Panic,
			_ => TensionState.Critical
		};

	//GAME DESIGN: anything that HELPS the player should use these individual direct EMAs (like spawning health item if health is generally low for a long time)
	private readonly EMA emaPlayerHealth = new(0.01f);
	private readonly EMA emaPlayerAmmo = new(0.05f);

	private readonly EMA emaTension = new(0.01f); //slow reaction for hysteresis
	private float baseTension; //rigid and only going forward, no need for EMA
	private float playerStrength; //EMA provided by emaTension

	//GAME DESIGN: anything that CHALLENGES the player should use this general tension to ramp up or down the difficulty and mood
	public float Tension { get; private set; }
	public float DifficultyModifier { get; private set; } //unused, not sure yet where to use it tbh

	private const int MAX_ZOMBIE_COUNT = 20;
	private const int MAX_ITEM_HEALTH_COUNT = 2;
	private const int MAX_ITEM_AMMO_COUNT = 5;
	private float zombieSpawnTimer;
	private float healthSpawnTimer;
	private float ammoSpawnTimer;

	public AIDirectorManager(GameplayState gameplayState) : base(gameplayState)
	{
		zombieSpawnTimer = CalculateZombieSpawnInterval();
		healthSpawnTimer = 30f;
		ammoSpawnTimer = 20f;
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		TensionUpdate(dt);

		//spawn health when HP is GENERALLY very low, as well as for ammo
		//for zombies, make them spawn very few when tension is low, but very frequent up to when tension is critical
		//consider the max counts to avoid flooding, then use timer variable (and dt) for timers

		zombieSpawnTimer -= dt;
		if (zombieSpawnTimer <= 0)
		{
			zombieSpawnTimer = CalculateZombieSpawnInterval();

			int currentZombieCount = gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(ZombieEntity)).Count;
			if (currentZombieCount < MAX_ZOMBIE_COUNT)
			{
				int zombiesToSpawn = CalculateZombiesToSpawn();
				for (int i = 0; i < zombiesToSpawn; i++)
				{
					SpawnZombie();
				}
			}
		}

		healthSpawnTimer -= dt;
		if (healthSpawnTimer <= 0 && emaPlayerHealth.Current < 0.5f)
		{
			int currentHealthItems = gameplayState.CurrentWorld.GetEntitiesByGroup("health").Count;

			if (currentHealthItems < MAX_ITEM_HEALTH_COUNT)
			{
				SpawnHealthItem();
				healthSpawnTimer = 30f;
			}
		}

		ammoSpawnTimer -= dt;
		if (ammoSpawnTimer <= 0 && emaPlayerAmmo.Current < 0.5f)
		{
			int currentAmmoItems = gameplayState.CurrentWorld.GetEntitiesByGroup("ammo").Count;

			if (currentAmmoItems < MAX_ITEM_AMMO_COUNT)
			{
				SpawnAmmoItem();
				ammoSpawnTimer = 20f;
			}
		}
	}

	private float CalculateZombieSpawnInterval()
	{
		// Tension 0.0 = 10s, Tension 1.0 = 1s
		return 10f - Tension * 9f;
	}

	private int CalculateZombiesToSpawn()
	{
		return CurrentState switch
		{
			TensionState.Calm => 1,
			TensionState.Tense => 2,
			TensionState.Panic => 3,
			TensionState.Critical => 4
		};
	}

	private Vector2 GetSpawnPosition()
	{
		var pos = gameplayState.GetManager<PlayerManager>().PlayerCharacter.Position;
		return gameplayState.GetManager<WaypointManager>().GetNodePosition(pos, 30, 35); //get guaranteed reachable far away position from player for spawning stuff (zombie, item, etc)
	}

	private void SpawnHealthItem()
	{
		gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
		{
			e.ItemType = ItemPickupEntity.ItemTypes.Health;
			e.Amount = 30;
			e.Position = GetSpawnPosition();
		});
	}

	private void SpawnAmmoItem()
	{
		gameplayState.CurrentWorld.SpawnEntity<ItemPickupEntity>(e =>
		{
			e.ItemType = RNG.Chance(0.7f) ? ItemPickupEntity.ItemTypes.AmmoSIG : ItemPickupEntity.ItemTypes.AmmoAK;
			e.Amount = 30;
			e.Position = GetSpawnPosition();
		});
	}

	private void SpawnZombie()
	{
		gameplayState.CurrentWorld.SpawnEntity<ZombieEntity>(e =>
		{
			e.Position = GetSpawnPosition() + new Vector2(RNG.Range(-0.1f, 0.1f), RNG.Range(-0.1f, 0.1f));
		});
	}

	private void TensionUpdate(float dt)
	{
		var gm = gameplayState.GetManager<GameplayManager>();
		if (!gm.Running)
			return;

		const float maxDeviation = 0.2f;

		var pc = gameplayState.GetManager<PlayerManager>().PlayerCharacter;

		emaPlayerHealth.AddSample((float)pc.HP / (float)pc.MaxHP);
		//for quick dev, guns and max ammos are guaranteed as non-zero
		emaPlayerAmmo.AddSample(pc.guns.Sum(p => (float)(p.CurrentAmmo + p.CurrentMaxAmmo) / (float)(p.MagSize + p.MaxAmmo)) / (float)pc.guns.Count); //TODO: add Signal<Gun> on gun fire for ammo change instead of continuous per-frame query

		var healthFactor = emaPlayerHealth.Current;
		var ammoFactor = emaPlayerAmmo.Current;

		playerStrength = healthFactor * 0.95f + ammoFactor * 0.05f;
		baseTension = GetBaseTensionRamp(1f - (gm.GameTime / gm.MaxGameTime)); //GameTime counts down, 0 = win ending, and the lower the number, the more intense the gameplay (zombie home invasion from midnight until dawn)

		var deviation = playerStrength - 0.5f;
		var newTension = baseTension + deviation * maxDeviation; //player's status "rides" the baseTension without straying too far

		DifficultyModifier = playerStrength;

		emaTension.AddSample(newTension);
		Tension = Math.Clamp(emaTension.Current, 0f, 1f);
	}

	private float GetBaseTensionRamp(float t)
	{
		//ease in out circ
		//slow ramp up at start, sudden spike at middle, then slow towards peak until the end
		return t < 0.5f ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * t, 2))) / 2
  						: (MathF.Sqrt(1 - MathF.Pow(-2 * t + 2, 2)) + 1) / 2;
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		ImGui.Text($"Tension: {Tension:F2} ({CurrentState})");
		ImGui.Text($"Base Tension: {baseTension:F2}");
		ImGui.Text($"Difficulty Modifier: {DifficultyModifier:F2}");
		ImGui.Text($"Player Strength: {playerStrength:F2}");
		ImGui.Text($"Player Health: {emaPlayerHealth.Current:F2}");
		ImGui.Text($"Player Ammo: {emaPlayerAmmo.Current:F2}");
	}
}
