using Main.Helpers;

namespace Main.Core;

public class MenuState : IGameState
{
	public void Enter()
	{

	}

	public void Exit()
	{

	}

	public void Update(float dt, float udt)
	{

	}

	public void Draw()
	{
		
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Play"))
			Game.Instance.GoToIngame();
	}
}