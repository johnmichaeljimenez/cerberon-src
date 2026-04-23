using Main.Core;
using Main.Helpers;

namespace Main.Gameplay.Managers;

public class WaypointManager : BaseManager
{
	public class Node
	{
		public Vector2 Position;
		public readonly List<Node> Connections = new();

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
	private readonly List<(Vector2, Vector2)> obstacleLines = new(); //from, to

	public WaypointManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public Node GetNearestNode(Vector2 point)
	{
		if (nodes.Count == 0) return null;

		Node nearest = null;
		float bestDistSq = float.MaxValue;

		foreach (var n in nodes)
		{
			var distSq = Vector2.DistanceSquared(n.Position, point);
			if (distSq < bestDistSq)
			{
				bestDistSq = distSq;
				nearest = n;
			}
		}

		return nearest;
	}

	public Vector2 GetNodePosition(Vector2 origin, float minDistance = 10f, float maxDistance = 20f) //useful for "crawling" the map and picking reasonable and reachable positions from point of origin (uses real travel distance, not euclidean)
	{
		var start = GetNearestVisibleNode(origin);

		if (start == null)
			return origin;

		var originToStart = Vector2.Distance(origin, start.Position);

		var distFromStart = new Dictionary<Node, float> { [start] = 0f }; //TODO: try to reuse collections
		var queue = new PriorityQueue<Node, float>();
		queue.Enqueue(start, 0f);

		var candidates = new List<Node>();
		var visited = new HashSet<Node> { start };

		var totalDist = originToStart;
		if (totalDist >= minDistance && totalDist <= maxDistance)
			candidates.Add(start);

		while (queue.Count > 0)
		{
			var current = queue.Dequeue();
			float currentDistFromStart = distFromStart[current];

			foreach (var neighbour in current.Connections)
			{
				if (!visited.Add(neighbour))
					continue;

				float edgeDist = Vector2.Distance(current.Position, neighbour.Position);
				float newDistFromStart = currentDistFromStart + edgeDist;

				distFromStart[neighbour] = newDistFromStart;

				float totalPathDist = originToStart + newDistFromStart;

				if (totalPathDist >= minDistance && totalPathDist <= maxDistance)
					candidates.Add(neighbour);

				if (totalPathDist <= maxDistance)
					queue.Enqueue(neighbour, newDistFromStart);
			}
		}

		if (candidates.Count == 0)
			return start.Position;

		var rnd = new Random(); // TODO: proper Random static class
		Node chosen = candidates[rnd.Next(candidates.Count)];

		return chosen.Position;
	}

	public void Move(Vector2 from, Vector2 to, List<Vector2> outputNodes)
	{
		outputNodes.Clear();

		if (nodes.Count == 0 || Vector2.DistanceSquared(from, to) < 0.001f)
		{
			outputNodes.Add(to);
			return;
		}

		if (IsVisible(from, to))
		{
			outputNodes.Add(to);
			return;
		}

		Node startNode = GetNearestVisibleNode(from);
		Node goalNode = GetNearestVisibleNode(to);

		if (startNode == null || goalNode == null)
		{
			outputNodes.Add(to);
			return;
		}

		if (startNode == goalNode)
		{
			outputNodes.Add(startNode.Position);
			outputNodes.Add(to);
			return;
		}

		var nodePath = FindPath(startNode, goalNode);

		if (nodePath == null || nodePath.Count == 0)
		{
			outputNodes.Add(startNode.Position);
			outputNodes.Add(to);
			return;
		}

		foreach (var node in nodePath)
		{
			outputNodes.Add(node.Position);
		}

		outputNodes.Add(to);
		//TODO: add string pulling algorithm to simplify path (ex. start -> a -> b -> c -> goal, and c is already visible, make it start -> c -> goal)
	}

	public void Bake(IEnumerable<Rectangle> rawObstacles, Vector2 worldSize, float characterRadius)
	{
		nodes.Clear();

		//we are going to make a DIY homemade Probabilistic Roadmap (PRM) instead of A* grid here
		// 1. take all static rectangle colliders and expand them by minkowski sum, then generate node points
		// 2. use poisson disc sampling to make random node points in the game world
		// 3. remove all node points that are inside rectangle colliders
		// 4. connect all node points that can directly "see" each other

		var distributionRadius = characterRadius * 2; //character units before the next scattered node
		var obstacles = new List<Rectangle>();

		obstacleLines.Clear();

		//step 1
		foreach (var i in rawObstacles)
		{
			var r = characterRadius;
			var bounds = i.Expand(r);
			nodes.AddRange(bounds.ToVector2List().Select(p => new Node(p)));

			//TODO: step 1.5: for long rectangles, add more nodes (ex. if characterRadius = 1, then every 1 unit for a 5x5 square, instead of 4 points only on corners, it will have +3 on each edge

			var checkBounds = i.Expand(r - 0.1f); //breathing room for avoiding epsilon-level false positives
			obstacles.Add(checkBounds);
			var list = new List<Vector2>();
			list.AddRange(checkBounds.ToVector2List());

			obstacleLines.Add((list[0], list[1]));
			obstacleLines.Add((list[1], list[2]));
			obstacleLines.Add((list[2], list[3]));
			obstacleLines.Add((list[3], list[0]));
		}

		// //step 2
		var poisson = PoissonDisc.Sample(new(-worldSize / 2, worldSize), distributionRadius); //TODO: add "seed" parameter for determinism (not needed yet)
		nodes.AddRange(poisson.Select(p => new Node(p)));

		//step 3
		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			foreach (var j in obstacles) //uses the expanded version
			{
				var n = nodes[i];
				if (Raylib.CheckCollisionPointRec(n.Position, j))
				{
					nodes.RemoveAt(i); //TIL that RemoveAt is more efficient than Remove
					break;
				}
			}
		}

		//step 4
		for (int a = 0; a < nodes.Count; a++)
		{
			for (int b = a + 1; b < nodes.Count; b++)
			{
				//TODO: optimize (no need for now), use spatial hashing
				var i = nodes[a];
				var j = nodes[b];

				var from = i.Position;
				var to = j.Position;
				var hit = false;
				var dist = (to - from).Length();

				if (dist >= distributionRadius * 2)
					continue;

				foreach (var k in obstacleLines)
				{
					Vector2 p = default;
					if (Raylib.CheckCollisionLines(from, to, k.Item1, k.Item2, ref p))
					{
						hit = true;
						break;
					}
				}

				if (hit)
					continue;

				//connect i and j here
				i.Connections.Add(j);
				j.Connections.Add(i);
			}
		}
	}

	public override void DrawDebug()
	{
		foreach (var i in nodes)
		{
			Raylib.DrawCircleV(i.Position, 0.5f, Colors.GREEN);
			foreach (var j in i.Connections)
			{
				Raylib.DrawLineV(i.Position, j.Position, Colors.GREEN);
			}
		}

		foreach (var line in obstacleLines)
		{
			Raylib.DrawLineV(line.Item1, line.Item2, Colors.RED);
		}
	}

	public bool IsVisible(Vector2 a, Vector2 b)
	{
		if (Vector2.DistanceSquared(a, b) < 0.001f) return true;

		Vector2 p = default;
		foreach (var line in obstacleLines)
		{
			if (Raylib.CheckCollisionLines(a, b, line.Item1, line.Item2, ref p))
			{
				return false;
			}
		}
		return true;
	}

	private Node GetNearestVisibleNode(Vector2 point)
	{
		if (nodes.Count == 0) return null;

		Node nearestVisible = null;
		Node nearestAny = null;
		var bestVisibleDistSq = float.MaxValue;
		var bestAnyDistSq = float.MaxValue;

		foreach (var n in nodes)
		{
			var distSq = Vector2.DistanceSquared(n.Position, point);

			if (distSq < bestAnyDistSq)
			{
				bestAnyDistSq = distSq;
				nearestAny = n;
			}

			if (distSq < bestVisibleDistSq && IsVisible(point, n.Position))
			{
				bestVisibleDistSq = distSq;
				nearestVisible = n;
			}
		}

		return nearestVisible ?? nearestAny;
	}

	private List<Node> FindPath(Node start, Node goal)
	{
		//TODO: try to reuse collections
		if (start == goal) return new List<Node> { start };

		var cameFrom = new Dictionary<Node, Node>();
		var gScore = new Dictionary<Node, float>();
		var openSet = new PriorityQueue<Node, float>();
		var closedSet = new HashSet<Node>();

		gScore[start] = 0f;
		openSet.Enqueue(start, Vector2.Distance(start.Position, goal.Position));

		while (openSet.Count > 0)
		{
			var current = openSet.Dequeue();

			if (closedSet.Contains(current)) continue;
			closedSet.Add(current);

			if (current == goal)
			{
				return ReconstructPath(cameFrom, current);
			}

			foreach (var neighbor in current.Connections)
			{
				float tentativeGScore = gScore[current] + Vector2.Distance(current.Position, neighbor.Position);

				if (!gScore.TryGetValue(neighbor, out float neighborGScore) || tentativeGScore < neighborGScore)
				{
					cameFrom[neighbor] = current;
					gScore[neighbor] = tentativeGScore;

					float fScore = tentativeGScore + Vector2.Distance(neighbor.Position, goal.Position);
					openSet.Enqueue(neighbor, fScore);
				}
			}
		}

		return null; // no path
	}

	private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
	{
		//TODO: try to reuse collections
		var path = new List<Node>();
		while (current != null)
		{
			path.Add(current);
			current = cameFrom.TryGetValue(current, out var prev) ? prev : null;
		}
		path.Reverse();
		return path;
	}
}