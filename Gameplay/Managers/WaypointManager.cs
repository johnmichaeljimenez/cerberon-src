using Main.Core;
using Main.Gameplay.Level;
using Main.Helpers;

namespace Main.Gameplay.Managers;

public class WaypointManager : BaseManager
{
	public class Node
	{
		public Vector2 Position;
		public readonly List<Node> Connections = new();
		public float Exposure = 0.5f;

		public Node(float x, float y)
		{
			Position = new(x, y);
		}

		public Node(Vector2 position)
		{
			Position = position;
		}
	}

	public const float PosterizeSize = 5f;
	public const float PosterizeStep = 1f / PosterizeSize;

	public List<Node> Nodes => nodes;
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
		var start = GetNearestNode(origin);

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

	public void Bake(IEnumerable<WorldCollider> rawObstacles, Vector2 worldSize, float characterRadius)
	{
		nodes.Clear();

		//we are going to make a DIY homemade Probabilistic Roadmap (PRM) instead of A* grid here
		// 1. take all static rectangle colliders and expand them by minkowski sum, then generate node points
		// 1.5 add nodes on walls that opposite each other to ensure chokepoints and doorways will not get skipped
		// 2. use poisson disc sampling to make random node points in the game world
		// 3. remove all node points that are inside rectangle colliders
		// 4. connect all node points that can directly "see" each other
		// 5. post process nodes and calculate gameplay-relevant data


		obstacleLines.Clear();

		//step 1
		foreach (var collider in rawObstacles)
		{
			var r = characterRadius;

			var expandedCorners = Utils.GetExpandedRectangleCorners(
				collider.Position, collider.Size, collider.Rotation, r);
			nodes.AddRange(expandedCorners.Select(p => new Node(p)));

			var obstacleEdges = Utils.GetRectangleEdges(
				collider.Position, collider.Size - (Vector2.One * 0.1f), collider.Rotation); //breathing room for avoiding epsilon-level false positives
			foreach (var edge in obstacleEdges)
			{
				obstacleLines.Add(edge);
			}
		}

		//step 1.5
		var midNodes = new List<Node>();
		for (int a = 0; a < obstacleLines.Count; a++)
		{
			for (int b = a + 1; b < obstacleLines.Count; b++)
			{
				var i = obstacleLines[a];
				var j = obstacleLines[b];

				//add only if:
				// "opposite enough" facing walls
				// near enough
				// so basically typical hallways and doorways

				var center1 = (i.Item1 + i.Item2) * 0.5f;
				var center2 = (j.Item1 + j.Item2) * 0.5f;
				
				var dir1 = i.Item2 - i.Item1;
				var dir2 = j.Item2 - j.Item1;

				var dist1 = dir1.Length();
				var dist2 = dir2.Length();

				if ((center1 - center2).Length() >= characterRadius * 4 || MathF.Abs(dist1 - dist2) > 0.1f)
					continue;
				
				var center = (center1 + center2) * 0.5f;

				midNodes.Add(new Node(center));
			}
		}

		nodes.AddRange(midNodes);

		// //step 2
		var distributionRadius = characterRadius * 4;
		var poisson = PoissonDisc.Sample(new(-worldSize / 2, worldSize), distributionRadius); // TODO: seed for determinism
		nodes.AddRange(poisson.Select(p => new Node(p)));

		//step 3
		for (int i = nodes.Count - 1; i >= 0; i--)
		{
			var nodePos = nodes[i].Position;
			foreach (var collider in rawObstacles)
			{
				float r = characterRadius;
				Vector2 expandedSize = collider.Size + new Vector2(r * 2f, r * 2f);

				if (Utils.IsPointInRotatedRectangle(nodePos, collider.Position, expandedSize, collider.Rotation))
				{
					nodes.RemoveAt(i);
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
				var dist = (to - from).Length();

				if (dist >= characterRadius * 8)
					continue;

				if (!IsVisible(from, to))
					continue;

				//connect i and j here
				i.Connections.Add(j);
				j.Connections.Add(i);
			}
		}

		//step 5
		ComputeExposures();
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

	private void ComputeExposures(float maxGraphDistance = 3f)
	{
		if (nodes.Count == 0) return;

		var rawScores = new Dictionary<Node, float>(nodes.Count);

		foreach (var node in nodes)
		{
			var score = CalculateRawExposureScore(node, maxGraphDistance);
			rawScores[node] = score;
		}

		// min-max normalize
		var minScore = float.MaxValue;
		var maxScore = float.MinValue;
		foreach (var score in rawScores.Values)
		{
			if (score < minScore) minScore = score;
			if (score > maxScore) maxScore = score;
		}

		foreach (var node in nodes)
		{
			var normalized = (maxScore - minScore > 0.001f)
				? (rawScores[node] - minScore) / (maxScore - minScore)
				: 0.5f;

			node.Exposure = MathF.Round(normalized / PosterizeStep) * PosterizeStep;
			node.Exposure = Math.Clamp(node.Exposure, 0f, 1f);
		}
	}

	private float CalculateRawExposureScore(Node start, float maxGraphDistance) //TODO: improve consistency
	{
		var visited = new HashSet<Node>();
		var queue = new Queue<(Node node, float distFromStart)>();
		queue.Enqueue((start, 0f));
		visited.Add(start);

		while (queue.Count > 0)
		{
			var (current, dist) = queue.Dequeue();

			if (dist >= maxGraphDistance)
				continue;

			foreach (var neighbor in current.Connections)
			{
				if (visited.Add(neighbor))
				{
					var edgeDist = Vector2.Distance(current.Position, neighbor.Position);
					var newDist = dist + edgeDist;
					if (newDist <= maxGraphDistance)
						queue.Enqueue((neighbor, newDist));
				}
			}
		}

		return visited.Count - 1;   //exclude self
	}
}