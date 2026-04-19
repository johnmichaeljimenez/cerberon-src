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
		AssetManager.GetSprite("birdie").Draw(new System.Numerics.Vector2(0, 0));

		//reference point check (to see that camera and birdie is at 0,0 with proper origin)
		Raylib.DrawCircle(0, 0, 1, Colors.RED);
	}

	public void DrawImGui()
	{
		if (ImGui.Button("Play"))
			Game.Instance.GoToIngame();
	}
}