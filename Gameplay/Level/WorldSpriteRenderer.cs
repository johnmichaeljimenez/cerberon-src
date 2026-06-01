using Cerberon.Core;

namespace Cerberon.Gameplay.Level;

[Serializable]
public class WorldSpriteRenderer //same with this
{
	public enum RenderTypes
	{
		Default,
		Tiled
	}

	[JsonProperty]
	public string SpriteID { get; set; }

	private Sprite _sprite = null;
	[JsonIgnore]
	public Sprite Sprite
	{
		get
		{
			if (_sprite == null)
				_sprite = AssetManager.GetSprite(SpriteID);

			return _sprite;
		}
	}

	public Vector2 Position { get; set; }
	public float Rotation { get; set; }
	public float Scale { get; set; } = 1;
	public Color Tint { get; set; } = Color.White;
	public int SortingGroup { get; set; }
	public int SortingIndex { get; set; }
	public float Parallax { get; set; }
	public RenderTypes RenderType { get; set; }
	public Vector2 TileSize { get; set; }

	public void Draw()
	{
		var pos = Position + Game.Instance.Camera.GetParallaxPosition(Position, Parallax);
		RenderingManager.BeginEnvironmentShader(RenderType == RenderTypes.Tiled, SortingGroup > 0, Sprite, TileSize);

		if (RenderType == RenderTypes.Tiled)
		{
			Sprite.DrawTiled(pos, TileSize, Rotation, Tint);
		}
		else
		{
			Sprite.Draw(pos, Scale, Rotation, Tint, Vector2.One * 0.5f);
		}
		
		Raylib.EndShaderMode();
	}
}
