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
	public int TextSize { get; set; } = 10;

	public string SpriteName { get; set; }
	private Sprite sprite;

	public Rectangle Rect => RenderingManager.GetRect(Position, Size);
	public float ScaledTextSize => TextSize * RenderingManager.Scale;
	public Rectangle GetTextRect
	{
		get
		{
			if (string.IsNullOrWhiteSpace(Text))
				return new Rectangle(Rect.X, Rect.Y, 0, 0);

			float scaledSize = ScaledTextSize;
			int textWidth = Raylib.MeasureText(Text, (int)scaledSize);
			int textHeight = (int)scaledSize;

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

	public void Init()
	{
		sprite = AssetManager.GetSprite(SpriteName);
	}

	public void Draw(bool hovered)
	{
		if (Clickable)
			Raylib.DrawRectangleLinesEx(Rect, 1, Color.White);

		var tint = Color.White.Value(Clickable && hovered ? 1.0f : 0.8f);
		if (sprite != null)
		{
			Raylib.DrawTexturePro(sprite.Texture, new Rectangle(0, 0, sprite.Width, sprite.Height), Rect, Vector2.Zero, 0, tint);
		}

		if (TextSize > 0 && !string.IsNullOrWhiteSpace(Text))
		{
			Raylib.DrawTextEx(AssetManager.Font, Text, GetTextRect.Position + Vector2.UnitY * 2, ScaledTextSize, 0, Color.Black); //shadow
			Raylib.DrawTextEx(AssetManager.Font, Text, GetTextRect.Position, ScaledTextSize, 0, tint);
		}
	}
}