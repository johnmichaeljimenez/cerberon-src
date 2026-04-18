using Main.Core;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;

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

		AddManager<CollisionManager>();
		AddManager<PlayerManager>();

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

		GetManager<PlayerManager>().SpawnPlayer(Vector2.Zero);
	}

	public void Exit()
	{
		CurrentWorld.Dispose();

		foreach (var i in managers)
		{
			i.Value.Dispose();
		}
	}

	public void Update(float dt)
	{
		CurrentWorld.Update(dt);

		foreach (var i in managers)
		{
			i.Value.Update(dt);
		}
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
		Raylib.DrawCircleLinesV(InputManager.MouseWorldPosition, 0.5f, Color.Green);
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Menu"))
			Game.Instance.GoToMenu();

		ImGui.Checkbox("Draw Debug", ref drawDebug);

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