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
		CurrentWorld = new World();
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
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Menu"))
			Game.Instance.GoToMenu();
	}
}

public class GameplayOptions
{
	//not sure what to put here tbh that I NEED immediately for any game genre (not arcade or puzzle, of course). what do you think?
}