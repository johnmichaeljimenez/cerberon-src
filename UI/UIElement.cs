using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class UIElement
{
	public string ID { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	public string Group { get; set; } = "";
	public bool Visible { get; set; } = true;
	public bool Clickable { get; set; }

	public Rectangle Rect => RenderingManager.GetRect(Position, Size);
}