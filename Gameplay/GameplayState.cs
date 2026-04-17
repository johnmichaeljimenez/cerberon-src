using Main.Core;
using Main.Gameplay.Entities;

namespace Main.Gameplay;

public class GameplayState : IGameState
{
	public readonly GameplayOptions options;

	public World CurrentWorld { get; private set; }

	public GameplayState(GameplayOptions options)
	{
		World.InitRegistry();
		this.options = options;
	}

	public void Enter()
	{
		CurrentWorld = SampleWorld.GetSampleWorldData(); //while I am too lazy to make a JSON
		CurrentWorld.Init();
		
		CurrentWorld.SpawnEntity<PlayerEntity>();
	}

	public void Exit()
	{
		CurrentWorld.Dispose();
	}

	public void Update(float dt)
	{
		CurrentWorld.Update(dt);
	}

	public void Draw()
	{
		CurrentWorld.Draw();

		Raylib.DrawCircleV(InputManager.MouseWorldPosition, 1, Color.Green);
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Menu"))
			Game.Instance.GoToMenu();
	}
}

public class GameplayOptions
{
	//difficulty, current save slot, is debug mode, and anything that I will need later
}