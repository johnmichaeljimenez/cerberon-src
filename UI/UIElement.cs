using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class UIElement
{
	public string ID { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	public bool Visible { get; set; } = true;

	public void Draw()
	{
		//TEST ONLY, will add sprites and text property
		var rect = RenderingManager.GetRect(Position, Size);
		var outline = rect.Expand(2);

		Raylib.DrawRectangleV(outline.Position, outline.Size, Color.White);
		Raylib.DrawRectangleV(rect.Position, rect.Size, Color.DarkGreen);
	}
}