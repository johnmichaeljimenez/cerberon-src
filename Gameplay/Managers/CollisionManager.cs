using Main.Gameplay;
using Main.Gameplay.Managers;

public class Wall
{
	public Vector2 From { get; set; }
	public Vector2 To { get; set; }
	public Vector2 Normal { get; set; }
	public Vector2 Midpoint { get; set; }
	public float Length { get; set; }
}

public class CollisionManager : BaseManager
{
	private readonly List<Wall> walls = new();

	public CollisionManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public void AddWalls(Vector2 pos, Vector2 size, List<Wall> walls)
	{
		// Top wall
		walls.Add(AddWall(pos, new Vector2(pos.X + size.X, pos.Y)));
		// Right wall
		walls.Add(AddWall(new Vector2(pos.X + size.X, pos.Y), new Vector2(pos.X + size.X, pos.Y + size.Y)));
		// Bottom wall
		walls.Add(AddWall(new Vector2(pos.X + size.X, pos.Y + size.Y), new Vector2(pos.X, pos.Y + size.Y)));
		// Left wall
		walls.Add(AddWall(new Vector2(pos.X, pos.Y + size.Y), pos));
	}

	//do clockwise order when making polygons
	public Wall AddWall(Vector2 from, Vector2 to)
	{
		var wall = new Wall
		{
			From = from,
			To = to
		};

		wall.Midpoint = (from + to) * 0.5f;
		Vector2 dir = to - from;
		wall.Length = dir.Length();

		if (wall.Length > 0.0001f)
		{
			wall.Normal = Vector2.Normalize(new Vector2(dir.Y, -dir.X));
		}
		else
		{
			wall.Normal = Vector2.Zero;
		}

		walls.Add(wall);
		return wall;
	}

	public void RemoveWall(Wall wall)
	{
		if (wall != null)
			walls.Remove(wall);
	}

	/// Usage example:
	/// Vector2 pos = myPosition;
	/// Vector2 delta = myVelocity * Time.deltaTime;
	/// if (collision.Move(pos, myRadius, ref delta))
	///     // optional: handle hit event
	/// myPosition += delta;
	public bool Move(Vector2 position, float radius, ref Vector2 velocity)
	{
		Vector2 proposed = position + velocity;
		bool collided = false;

		foreach (var w in walls)
		{
			Vector2 d = proposed - w.Midpoint;

			if (Vector2.Dot(w.Normal, d) > 0f)
			{
				Vector2 closest = GetClosestPointOnSegment(w.From, w.To, proposed);
				Vector2 cv = closest - proposed;
				float cd = cv.Length();

				if (cd <= radius && cd > 0.00001f)
				{
					Vector2 pushDir = Vector2.Normalize(cv);
					proposed += pushDir * (cd - radius);
					collided = true;
				}
			}
		}

		velocity = proposed - position;
		return collided;
	}

	public bool Linecast(Vector2 from, Vector2 to, out float distance)
	{
		distance = Vector2.Distance(from, to);
		bool hit = false;
		float minDistSq = float.MaxValue;

		Vector2 dir = from - to;
		float lenSq = dir.LengthSquared();
		if (lenSq < 0.0001f)
		{
			distance = 0f;
			return false;
		}

		dir = Vector2.Normalize(dir);

		foreach (var w in walls) //optimization like spatial hashing is a future me's problem (I have full exp on that)
		{
			if (Vector2.Dot(w.Normal, dir) > 0f)
			{
				if (TryGetLineIntersection(from, to, w.From, w.To, out Vector2 intersection))
				{
					float distSq = Vector2.DistanceSquared(from, intersection);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						hit = true;
					}
				}
			}
		}

		if (hit)
		{
			distance = MathF.Sqrt(minDistSq);
			return true;
		}

		return false;
	}

	private static Vector2 GetClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
	{
		Vector2 ab = b - a;
		float abLenSq = ab.LengthSquared();
		if (abLenSq == 0f) return a;

		float t = Vector2.Dot(p - a, ab) / abLenSq;
		t = Math.Clamp(t, 0f, 1f);
		return a + ab * t;
	}

	private static bool TryGetLineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection)
	{
		intersection = Vector2.Zero;

		float A1 = a2.Y - a1.Y;
		float B1 = a1.X - a2.X;
		float C1 = A1 * a1.X + B1 * a1.Y;

		float A2 = b2.Y - b1.Y;
		float B2 = b1.X - b2.X;
		float C2 = A2 * b1.X + B2 * b1.Y;

		float det = A1 * B2 - A2 * B1;
		if (Math.Abs(det) < 0.00001f) return false;

		float x = (B2 * C1 - B1 * C2) / det;
		float y = (A1 * C2 - A2 * C1) / det;

		if (IsPointOnSegment(a1, a2, x, y) && IsPointOnSegment(b1, b2, x, y))
		{
			intersection = new Vector2(x, y);
			return true;
		}

		return false;
	}

	private static bool IsPointOnSegment(Vector2 p1, Vector2 p2, float x, float y)
	{
		float minX = Math.Min(p1.X, p2.X);
		float maxX = Math.Max(p1.X, p2.X);
		float minY = Math.Min(p1.Y, p2.Y);
		float maxY = Math.Max(p1.Y, p2.Y);

		return x >= minX - 0.0001f && x <= maxX + 0.0001f &&
			   y >= minY - 0.0001f && y <= maxY + 0.0001f;
	}
}