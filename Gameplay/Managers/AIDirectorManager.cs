using Main.Core;
using Main.Gameplay.Entities;
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
	private readonly EMA emaPlayerSkill = new(0.09f);

	private readonly EMA emaTension = new(0.08f);

	public float Tension { get; private set; }

	private const int MAX_ZOMBIE_COUNT = 20;
	private const int MAX_ITEM_HEALTH_COUNT = 2;
	private const int MAX_ITEM_AMMO_COUNT = 5;

	private float zombieSpawnTimer;
	private float healthSpawnTimer;
	private float ammoSpawnTimer;

	private float recentKillCount;
	private const float KILL_WINDOW_SECONDS = 30f;

	public AIDirectorManager(GameplayState gameplayState) : base(gameplayState)
	{
		zombieSpawnTimer = 9f;
		healthSpawnTimer = 28f;
		ammoSpawnTimer = 22f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		gameplayState.CurrentWorld.OnEntityDespawn.Subscribe(e =>
		{
			if (e is ZombieEntity) recentKillCount++;
		}).AddTo(disposables);
	}

	public override void Update(float dt, float udt)
	{
		if (PauseHandler.IsPaused) return;

		base.Update(dt, udt);

		TensionUpdate(dt);

		if (emaPlayerHealth.Current <= 0) return;
		
		//spawn health when HP is GENERALLY very low, as well as for ammo
		//for zombies, make them spawn very few when tension is low, but very frequent up to when tension is critical
		//consider the max counts to avoid flooding, then use timer variable (and dt) for timers

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
		if (healthSpawnTimer <= 0 && emaPlayerHealth.Current < 0.55f)
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("health").Count < MAX_ITEM_HEALTH_COUNT)
			{
				SpawnHealthItem();
				healthSpawnTimer = 28f;
			}
		}

		ammoSpawnTimer -= dt;
		if (ammoSpawnTimer <= 0 && emaPlayerHealth.Current < 0.7f)
		{
			if (gameplayState.CurrentWorld.GetEntitiesByGroup("ammo").Count < MAX_ITEM_AMMO_COUNT)
			{
				SpawnAmmoItem();
				ammoSpawnTimer = 22f;
			}
		}
	}

	private void TensionUpdate(float dt)
	{
		var pc = gameplayState.GetManager<PlayerManager>().PlayerCharacter;

		emaPlayerHealth.AddSample((float)pc.HP / pc.MaxHP);

		recentKillCount = Math.Max(0f, recentKillCount - (1f / KILL_WINDOW_SECONDS) * dt);
		float killsPerMinute = recentKillCount * (60f / KILL_WINDOW_SECONDS);
		float skillScore = Math.Clamp(killsPerMinute / 45f, 0f, 3f);
		skillScore = skillScore * 0.65f + pc.Weapons.Accuracy * 0.35f;

		emaPlayerSkill.AddSample(skillScore);

		float recentKillsPerSec = recentKillCount / KILL_WINDOW_SECONDS;

		float performance =
			emaPlayerSkill.Current * 0.55f +      // skill + accuracy
			emaPlayerHealth.Current * 0.35f +     // staying healthy = skilled
			(recentKillsPerSec * 0.8f);           // fast killing = very skilled

		float struggle = (1f - emaPlayerHealth.Current) * 1.5f;   // strong relief when taking damage

		float desiredTension = performance - struggle;
		desiredTension = Math.Clamp(desiredTension, 0f, 1f);

		emaTension.AddSample(desiredTension);
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
			gameplayState.GetManager<PlayerManager>().PlayerCharacter.Position, 28f, 36f);

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
		ImGui.SeparatorText("AI Director");

		ImGui.Text($"Tension: {Tension:F3} ({CurrentState})");
		ImGui.ProgressBar(Tension, new System.Numerics.Vector2(340, 25));

		ImGui.Text($"Player Health (EMA): {emaPlayerHealth.Current:F2}");
		ImGui.Text($"Player Skill (EMA):  {emaPlayerSkill.Current:F2}");
		ImGui.Text($"Recent kills (last {KILL_WINDOW_SECONDS}s): {recentKillCount:F1} ({recentKillCount / KILL_WINDOW_SECONDS:F2}/s)");

		if (ImGui.CollapsingHeader("Raw Factors", ImGuiTreeNodeFlags.DefaultOpen))
		{
			float recentKillsPerSec = recentKillCount / KILL_WINDOW_SECONDS;
			float performance = emaPlayerSkill.Current * 0.55f + emaPlayerHealth.Current * 0.35f + (recentKillsPerSec * 0.8f);
			float struggle = (1f - emaPlayerHealth.Current) * 1.1f;

			ImGui.Text($"Performance: {performance:F3}");
			ImGui.Text($"Struggle (damage): {struggle:F3}");
			ImGui.Separator();
			ImGui.Text($"→ Desired Tension (before EMA): {performance - struggle:F3}");
		}
	}
}