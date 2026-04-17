using Main.Core;

namespace Main.Gameplay.Entities;

public class WallEntity : BaseEntity //simple solid rectangular blocker
{
	[JsonProperty]
	public Vector2 Size = Vector2.One;

	public override void Draw()
	{
		Raylib.DrawRectangleV(Position, Size, Color.Black);
	}
}