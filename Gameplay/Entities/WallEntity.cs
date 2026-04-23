using Main.Core;
using Main.Effects;
using Main.Helpers;

namespace Main.Gameplay.Entities;

public class WallEntity : BaseEntity //simple solid rectangular blocker
{
	[JsonProperty]
	public Vector2 Size = Vector2.One;

	public Rectangle RectangleBounds => new Rectangle()
	{
		Position = Position,
		Size = Size
	};

	private readonly List<Wall> walls = new();
	private Shadow shadow;

	public override void Init(GameplayState gameplayState)
	{
		base.Init(gameplayState);

		gameplayState.GetManager<CollisionManager>().AddWalls(Position, Size, walls);
		shadow = LightingSystem.AddShadow(Position, Size);
	}

	public override void Dispose()
	{
		base.Dispose();

		LightingSystem.RemoveShadow(shadow);
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
			Utils.DrawLineEx(i.From, i.To, i.Midpoint, i.Normal, Colors.RED);
		}
	}
}