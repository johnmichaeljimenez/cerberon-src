using Cerberon.Helpers;

namespace Cerberon.Core;

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
		Raylib.ClearBackground(Color.DarkGray);
	}

	public void DrawImGui()
	{
		// if (ImGui.Button("Play"))
		// 	Game.Instance.GoToIngame(null);
	}
}