using Main.Core;

namespace Main.Gameplay;

public class GameplayState : IGameState
{
	public readonly GameplayOptions options;

	public GameplayState()
	{
		World.InitRegistry();
	}

	public GameplayState(GameplayOptions options)
	{
		this.options = options;
	}

	public void Enter()
	{
		
	}

	public void Exit()
	{
		
	}

	public void Update(float dt)
	{
		
	}
	public void Draw()
	{
		
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