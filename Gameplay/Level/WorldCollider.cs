using Main.Gameplay.Managers;

namespace Main.Gameplay.Level;

[Serializable]
//TODO: try to reuse Wall class
public class WorldCollider //no need for real entities for static environment stuff like these
{
	public Vector2 Position { get; set; }
	public Vector2 Size { get; set; }
	public float Rotation { get; set; }
	public Wall.WallFlags Flags { get; set; }
	public CollisionHeight Height { get; set; }
}
