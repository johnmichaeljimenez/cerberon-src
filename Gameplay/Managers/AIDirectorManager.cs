using Main.Core;
using Main.Gameplay.Entities;
using Main.Gameplay.Entities.Player;
using Main.Helpers;

namespace Main.Gameplay.Managers;

//TODO: make all magic numbers editable outside
//TODO: make items and enemies despawn when far enough
public class AIDirectorManager : BaseManager
{
	public enum TensionState { Calm, Tense, Panic, Critical }

	public TensionState CurrentState =>
		Tension switch
		{
			< 0.3f => TensionState.Calm,
			< 0.6f => TensionState.Tense,
			< 0.85f => TensionState.Panic,
			_ => TensionState.Critical
		};

	private readonly EMA emaPlayerHealth = new(0.05f);
	private readonly EMA emaPlayerAccuracy = new(0.005f);
	private readonly EMA emaKillCount = new(0.003f);
	private readonly EMA emaPlayerHurt = new(0.003f);

	private readonly EMA emaTension = new(0.005f);

	public float Tension { get; private set; }

	private const int MAX_ZOMBIE_COUNT = 20;
	private const int MAX_ITEM_HEALTH_COUNT = 3;
	private const int MAX_ITEM_AMMO_COUNT = 5;

	private float zombieSpawnTimer;
	private float healthSpawnTimer;
	private float ammoSpawnTimer;

	private PlayerEntity player;

	public AIDirectorManager(GameplayState gameplayState) : base(gameplayState)
	{
		zombieSpawnTimer = 10f;
		healthSpawnTimer = 0f;
		ammoSpawnTimer = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();

		player = gameplayState.GetManager<PlayerManager>().PlayerCharacter;

		gameplayState.CurrentWorld.OnEntityDespawn.Subscribe(e =>
		{
			if (e is ZombieEntity z && z.HP <= 0)
			{
				emaKillCount.AddSample(60.0f); //large bump to compensate for decay (add must be faster than reduction)
			}
		}).AddTo(disposables);

		player.OnTakeDamage.Subscribe(dmg =>
		{
			emaPlayerHurt.AddSample(dmg * 30);
		}).AddTo(disposables);
	}

	public override void Update(float dt, float udt)
	{
		if (PauseHandler.IsPaused) return;

		base.Update(dt, udt);

		TensionUpdate(dt);

		if (emaPlayerHealth.Current <= 0) return;

		zombieSpawnTimer -= dt;
		if (zombieSpawnTimer <= 0)
		{
			zombieSpawnTimer = CalculateZombieSpawnInterval();
			int current = gameplayState.CurrentWorld.GetEntitiesByGroup(nameof(ZombieEntity)).Count;
			if (current < MAX_ZOMBIE_COUNT)
			{
				int toSpawn = CalculateZombiesToSpawn();
				for (int i = 0; i < toSpawn; i++) SpawnZombie();
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
		if (ammoSpawnTimer <= 0 && emaPlayerHealth.Current < 0.7f)
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("ammo").Count < MAX_ITEM_AMMO_COUNT)
			{
				SpawnAmmoItem();
				ammoSpawnTimer = 15f;
			}
		}
	}

	private void TensionUpdate(float dt)
	{
		emaKillCount.AddSample(0f);
		emaPlayerHurt.AddSample(0f);

		emaPlayerAccuracy.AddSample(player.Weapons.Accuracy);
		emaPlayerHealth.AddSample((float)player.HP / player.MaxHP);

		var newTension = emaKillCount.Current * 0.4f +
						 emaPlayerAccuracy.Current * 0.1f + 
						 emaPlayerHealth.Current * 0.5f;

		newTension -= emaPlayerHurt.Current * 0.5f;

		newTension = Math.Clamp(newTension, 0f, 1f);
		emaTension.AddSample(newTension);
		
		Tension = emaTension.Current;
	}

	private float CalculateZombieSpawnInterval() => 9.5f - Tension * 7.8f;
	private int CalculateZombiesToSpawn() =>
		CurrentState switch
		{
			TensionState.Calm => 1,
			TensionState.Tense => 2,
			TensionState.Panic => 3,
			TensionState.Critical => 4,
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
		e.ItemType = RNG.Chance(0.65f) ? ItemPickupEntity.ItemTypes.AmmoSIG : ItemPickupEntity.ItemTypes.AmmoAK;
		e.Amount = 25;
		e.Position = GetSpawnPosition();
	});

	private void SpawnZombie() => gameplayState.CurrentWorld.SpawnEntity<ZombieEntity>(e =>
	{
		e.Position = GetSpawnPosition() + RNG.Position(0.2f);
	});

	public override void DrawImGui()
	{
		base.DrawImGui();
		ImGui.SeparatorText($"AI Director: ({CurrentState})");
		ImGui.ProgressBar(Tension, new(340, 25), $"Overall Tension: {emaTension.Current:F2}");

		ImGui.SeparatorText("Data");
		ImGui.ProgressBar(emaKillCount.Current, new(340, 25), $"Kill Rate: {emaKillCount.Current:F2}");
		ImGui.ProgressBar(emaPlayerAccuracy.Current, new(340, 25), $"Accuracy: {emaPlayerAccuracy.Current:F2}");
		ImGui.ProgressBar(emaPlayerHealth.Current, new(340, 25), $"Health: {emaPlayerHealth.Current:F2}");
		ImGui.ProgressBar(emaPlayerHurt.Current, new(340, 25), $"Damage Taken: {emaPlayerHurt.Current:F2}");
	}
}