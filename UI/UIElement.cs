using Main.Core;
using Main.Helpers;

namespace Main.UI;

public class UIElement
{
	public enum Alignment
	{
		Left = -1,
		Center = 0,
		Right = 1
	}

	public string ID { get; set; }
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	public string Group { get; set; } = "";
	public bool Visible { get; set; } = true;
	public bool Clickable { get; set; }

	public string Text { get; set; }
	public Alignment Align { get; set; }
	public int TextSize { get; set; } = 20;

	public Rectangle Rect => RenderingManager.GetRect(Position, Size);
	public Rectangle GetTextRect
	{
		get
		{
			if (string.IsNullOrWhiteSpace(Text))
				return new Rectangle(Rect.X, Rect.Y, 0, 0);

			int textWidth = Raylib.MeasureText(Text, TextSize);
			int textHeight = TextSize;

			float x = Rect.X;
			switch (Align)
			{
				case Alignment.Center:
					x = Rect.X + (Rect.Width - textWidth) / 2f;
					break;
				case Alignment.Right:
					x = Rect.X + Rect.Width - textWidth;
					break;
			}

			float y = Rect.Y + (Rect.Height - textHeight) / 2f;

			return new Rectangle(x, y, textWidth, textHeight);
		}
	}

	public void Draw(bool hovered)
	{
		var tint = Color.White.Value(hovered? 1.0f : 0.8f);
		if (TextSize > 0 && !string.IsNullOrWhiteSpace(Text))
		{
			Raylib.DrawTextEx(AssetManager.Font, Text, GetTextRect.Position + Vector2.UnitY * 2, TextSize, 0, Color.Black); //shadow
			Raylib.DrawTextEx(AssetManager.Font, Text, GetTextRect.Position, TextSize, 0, tint);
		}
	}
}