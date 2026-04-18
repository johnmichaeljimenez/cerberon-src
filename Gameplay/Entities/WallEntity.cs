using Main.Core;

namespace Main.Gameplay.Entities;

public class WallEntity : BaseEntity //simple solid rectangular blocker
{
	[JsonProperty]
	public Vector2 Size = Vector2.One;

	private readonly List<Wall> walls = new();

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		gameplayState.GetManager<CollisionManager>().AddWalls(Position, Size, walls);
	}

	public override void Dispose()
	{
		base.Dispose();

		foreach (var i in walls)
		{
			gameplayState.GetManager<CollisionManager>().RemoveWall(i);
		}
	}

	public override void Draw()
	{
		Raylib.DrawRectangleV(Position, Size, Color.Black);
	}

	public override void DrawDebug()
	{
		base.DrawDebug();

		foreach (var i in walls)
		{
			Utils.DrawLineEx(i.From, i.To, i.Midpoint, i.Normal, Color.Red);
		}
	}
}