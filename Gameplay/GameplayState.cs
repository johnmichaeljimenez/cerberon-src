using Main.Core;
using Main.Effects;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;
using Main.Helpers;
using Main.UI;

namespace Main.Gameplay;

public class GameplayState : IGameState
{
	public readonly GameplayOptions options;

	private bool isPaused;

	private readonly Dictionary<Type, BaseManager> managers = new();

	public World CurrentWorld { get; private set; }

	private bool drawDebug = false;

	public GameplayState(GameplayOptions options)
	{
		World.InitRegistry();
		this.options = options;

		LightingSystem.Clear();
		AddManager<CollisionManager>();
		AddManager<PlayerManager>();
		AddManager<GameplayManager>();
		AddManager<WaypointManager>();
		AddManager<AIDirectorManager>();

		RenderingManager.ResetAllFilters();

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
		string jsonText = File.ReadAllText("Assets/Levels/SampleScene.json"); //test
		CurrentWorld = jsonText.FromJson<World>();
		CurrentWorld.Init(this);

		GetManager<PlayerManager>().SpawnPlayer(CurrentWorld.WorldSettings.PlayerSpawnPoint);

		foreach (var i in managers)
		{
			i.Value.OnEnter();
		}
	}

	public void Exit()
	{
		CurrentWorld.Dispose();

		foreach (var i in managers)
		{
			i.Value.Dispose();
		}

		RenderingManager.ResetAllFilters();
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

		if (InputManager.IsPressed(InputAction.Pause))
		{
			PauseGame(!isPaused);
		}
	}

	public void PauseGame(bool paused)
	{
		isPaused = paused;

		if (isPaused)
		{
			PauseHandler.Pause("ingame");
			UIManager.ShowScreen<PauseMenuScreen>(this, false);
		}
		else
		{
			PauseHandler.Unpause("ingame");
			if (UIManager.Current is PauseMenuScreen)
				UIManager.Back();
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
		Raylib.DrawCircleLinesV(InputManager.MouseWorldPosition, 0.5f, Colors.GREEN);

		foreach (var i in managers)
		{
			i.Value.DrawDebug();
		}
	}

	public void DrawImGui()
	{
		if (ImGui.BeginTabBar("Gameplay"))
		{
			if (ImGui.BeginTabItem("Main"))
			{
				if (ImGui.Button("Menu"))
					Game.Instance.GoToMenu();

				ImGui.Checkbox("Draw Debug", ref drawDebug);

				ImGui.Text($"Paused? {PauseHandler.IsPaused}");
				if (ImGui.Button("Pause"))
				{
					if (PauseHandler.IsPaused)
						PauseHandler.Unpause("test");
					else
						PauseHandler.Pause("test");
				}

				ImGui.EndTabItem();
			}

			if (ImGui.BeginTabItem("World"))
			{
				CurrentWorld.DrawImGui();
				ImGui.EndTabItem();
			}

			foreach (var i in managers)
			{
				if (ImGui.BeginTabItem(i.Key.Name.Replace("Manager", "")))
				{
					i.Value.DrawImGui();
					ImGui.EndTabItem();
				}
			}

			ImGui.EndTabBar();
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