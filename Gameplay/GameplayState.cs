using Main.Core;
using Main.Effects;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;
using Main.Helpers;

namespace Main.Gameplay;

public class GameplayState : IGameState
{
	public readonly GameplayOptions options;

	private readonly Dictionary<Type, BaseManager> managers = new();

	public World CurrentWorld { get; private set; }

	private bool drawDebug = true;

	public GameplayState(GameplayOptions options)
	{
		World.InitRegistry();
		this.options = options;

		LightingSystem.Clear();
		AddManager<CollisionManager>();
		AddManager<PlayerManager>();
		AddManager<GameplayManager>();
		AddManager<WaypointManager>();

		foreach (var i in managers)
		{
			i.Value.Init();
		}
	}

	private void AddManager<T>() where T : BaseManager
	{
		var manager = (T)Activator.CreateInstance(typeof(T), this);
		managers[typeof(T)] = manager;
	}

	public void Enter()
	{
		CurrentWorld = SampleWorld.GetSampleWorldData(); //while I am too lazy to make a JSON
		CurrentWorld.Init(this);

		GetManager<PlayerManager>().SpawnPlayer(CurrentWorld.WorldSettings.PlayerSpawnPoint);
	}

	public void Exit()
	{
		CurrentWorld.Dispose();

		foreach (var i in managers)
		{
			i.Value.Dispose();
		}
		
		LightingSystem.Clear();
	}

	public void Update(float dt, float udt)
	{
		CurrentWorld.Update(dt, udt);

		foreach (var i in managers)
		{
			i.Value.Update(dt, udt);
		}
		
		CurrentWorld.LateUpdate(dt, udt);
	}

	public void Draw()
	{
		CurrentWorld.Draw();
		
		if (drawDebug)
		{
			DrawDebug();
		}
	}

	public void DrawDebug()
	{
		CurrentWorld.DrawDebug();
		Raylib.DrawCircleLinesV(InputManager.MouseWorldPosition, 0.5f, Colors.GREEN);

		foreach (var i in managers)
		{
			i.Value.DrawDebug();
		}
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Menu"))
			Game.Instance.GoToMenu();

		ImGui.Checkbox("Draw Debug", ref drawDebug);

		CurrentWorld.DrawImGui();

		foreach (var i in managers)
		{
			i.Value.DrawImGui();
		}
	}

	public T GetManager<T>() where T : BaseManager
	{
		return managers[typeof(T)] as T;
	}
}

public class GameplayOptions
{
	//difficulty, current save slot, is debug mode, and anything that I will need later
}