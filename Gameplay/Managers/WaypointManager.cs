using Main.Core;

namespace Main.Gameplay.Managers;

public class WaypointManager : BaseManager
{
	public class Node
	{
		public Vector2 Position;

		public Node(float x, float y)
		{
			Position = new(x, y);
		}

		public Node(Vector2 position)
		{
			Position = position;
		}
	}

	private readonly List<Node> nodes = new();

	public WaypointManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void Bake(IEnumerable<Rectangle> obstacles, float characterRadius)
	{
		nodes.Clear();

		//we are going to make a DIY homemade Probabilistic Roadmap (PRM) instead of A* grid here
		// 1. take all static rectangle colliders and expand them by minkowski sum, then generate node points
		// 2. use poisson disc sampling to make random node points in the game world
		// 3. remove all node points that are inside rectangle colliders
		// 4. connect all node points that can directly "see" each other

		//step 1
		foreach (var i in obstacles)
		{
			var bounds = new Rectangle(                     //Minkowski sum
				new Vector2(i.Position.X - characterRadius,
							i.Position.Y - characterRadius),
				new Vector2(i.Size.X + 2 * characterRadius,
							i.Size.Y + 2 * characterRadius)
			);

			nodes.Add(new(bounds.Position.X, bounds.Position.Y));
			nodes.Add(new(bounds.Position.X + bounds.Size.X, bounds.Position.Y));
			nodes.Add(new(bounds.Position.X + bounds.Size.X, bounds.Position.Y + bounds.Size.Y));
			nodes.Add(new(bounds.Position.X, bounds.Position.Y + bounds.Size.Y));
		}

		//TODO: step 2

		//step 3
		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			foreach (var j in obstacles) //not sure if I should use obstacles or the expanded version
			{
				var n = nodes[i];
				if (Raylib.CheckCollisionPointRec(n.Position, j))
				{
					nodes.Remove(n);
					break;
				}
			}
		}

		//TODO: step 4
	}

	public override void Dispose()
	{
		base.Dispose();
		nodes.Clear();
	}

	public override void DrawDebug()
	{
		foreach (var i in nodes)
		{
			Raylib.DrawCircleV(i.Position, 0.5f, Colors.GREEN);
		}
	}
}