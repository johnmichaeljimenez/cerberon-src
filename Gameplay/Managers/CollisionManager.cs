using Main.Gameplay;
using Main.Gameplay.Entities;
using Main.Gameplay.Managers;
using Main.Helpers;

public class Wall
{
	[Flags]
	public enum WallFlags
	{
		None = 0,
		Shadows = 1 << 0,
		DirectLineOfSight = 1 << 1,
		DrawOverlay = 1 << 2
	}

	public Vector2 From { get; set; }
	public Vector2 To { get; set; }
	public Vector2 Normal { get; set; }
	public Vector2 Midpoint { get; set; }
	public float Length { get; set; }
	public WallFlags Flags { get; set; }
}

public struct LinecastHit
{
	public Wall Wall;
	public CircleBody Body;
	public float Distance;
	public Vector2 From;
	public Vector2 HitPosition;
}

public class CircleBody
{
	[Flags]
	public enum States
	{
		None = 0,
		FullyDisabled = 1 << 0,
		Enabled = 1 << 1,
		DisabledMoveCheck = 1 << 2,
		DisabledLinecast = 1 << 3
	}

	public Vector2 Position;
	public float Radius;
	public BaseEntity SourceEntity;
	public States EnableState = States.Enabled;
}

public class CollisionManager : BaseManager
{
	private readonly List<Wall> walls = new(); //static walls, the most dynamic it can do is to enable/disable, no real movable stuff
	private readonly List<CircleBody> bodies = new();

	public CollisionManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public CircleBody AddBody(Vector2 pos, float radius, BaseEntity sourceEntity)
	{
		var body = new CircleBody()
		{
			Position = pos,
			Radius = radius,
			SourceEntity = sourceEntity
		};

		bodies.Add(body);
		return body;
	}

	public void RemoveBody(CircleBody body)
	{
		bodies.Remove(body);
	}

	public void AddWalls(
		Vector2 pos,
		Vector2 size,
		List<Wall> walls,
		Wall.WallFlags flags,
		bool invert = false,
		float rotation = 0f,
		Vector2 origin = default)
	{
		if (origin == default) origin = new Vector2(0.5f, 0.5f);

		var corners = Utils.GetRectangleCorners(pos, size, rotation, origin);

		Wall Add(Vector2 a, Vector2 b) =>
			invert ? AddWall(b, a, flags) : AddWall(a, b, flags);

		walls.Add(Add(corners[0], corners[1]));     // top
		walls.Add(Add(corners[1], corners[2]));     // right
		walls.Add(Add(corners[2], corners[3]));     // bottom
		walls.Add(Add(corners[3], corners[0]));     // left
	}

	//do clockwise order when making polygons to make them point outward, otherwise do it inward for maximum level boundaries
	public Wall AddWall(Vector2 from, Vector2 to, Wall.WallFlags flags)
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
	public bool Move(CircleBody body, ref Vector2 velocity)
	{
		Vector2 proposed = body.Position + velocity;
		bool collided = false;

		foreach (var w in walls)
		{
			Vector2 d = proposed - w.Midpoint;

			if (Vector2.Dot(w.Normal, d) > 0f) //backface culling optimizes it and also ensures that it doesn't do collision checks on stray walls. 99% of walls must be enclosed as polygons (whether normal or inverted polygons).
			{
				Vector2 closest = GetClosestPointOnSegment(w.From, w.To, proposed);
				Vector2 cv = closest - proposed;
				float cd = cv.Length();

				if (cd <= body.Radius && cd > 0.00001f)
				{
					Vector2 pushDir = Vector2.Normalize(cv);
					proposed += pushDir * (cd - body.Radius);
					collided = true;
				}
			}
		}

		velocity = proposed - body.Position;
		return collided;
	}

	//TODO: merge with Move() above
	public bool MoveRepelBody(CircleBody body, ref Vector2 velocity)
	{
		Vector2 proposed = body.Position + velocity;
		bool collided = false;

		foreach (var other in bodies)
		{
			if (body == other || other.EnableState.HasFlag(CircleBody.States.FullyDisabled) || other.EnableState.HasFlag(CircleBody.States.DisabledMoveCheck)) continue;

			Vector2 delta = proposed - other.Position;
			float distSq = delta.LengthSquared();
			float minDistance = body.Radius + other.Radius;

			if (distSq < minDistance * minDistance && distSq > 0.00001f)
			{
				float dist = MathF.Sqrt(distSq);
				Vector2 pushDir = delta / dist;

				proposed = other.Position + pushDir * minDistance;
				collided = true;
			}
		}

		velocity = proposed - body.Position;

		return collided;
	}

	public bool Linecast(Vector2 from, Vector2 to, out LinecastHit hitInfo, CircleBody fromBody = null)
	{
		hitInfo = new LinecastHit
		{
			From = from
		};

		float totalLengthSq = Vector2.DistanceSquared(from, to);
		if (totalLengthSq < 0.0001f)
		{
			hitInfo.Distance = 0f;
			return false;
		}

		float totalLength = MathF.Sqrt(totalLengthSq);
		Vector2 rayDir = (to - from) / totalLength;

		float minDistSq = float.MaxValue;
		bool hasHit = false;

		Vector2 dirForCulling = from - to;
		foreach (var w in walls)
		{
			if (Vector2.Dot(w.Normal, dirForCulling) > 0f)
			{
				Vector2 intersection = default;
				if (Raylib.CheckCollisionLines(from, to, w.From, w.To, ref intersection)) //I didn't know this is already available for Raylib-cs
				{
					float distSq = Vector2.DistanceSquared(from, intersection);
					if (distSq < minDistSq)
					{
						minDistSq = distSq;
						hasHit = true;
						hitInfo.Wall = w;
						hitInfo.Body = null;
						hitInfo.HitPosition = intersection;
						hitInfo.Distance = MathF.Sqrt(distSq);
					}
				}
			}
		}

		foreach (var other in bodies)
		{
			if (other == null || other == fromBody || other.EnableState == CircleBody.States.FullyDisabled || other.EnableState.HasFlag(CircleBody.States.DisabledLinecast)) continue;

			Vector2 oc = from - other.Position;
			float a = 1f;
			float b = 2f * Vector2.Dot(rayDir, oc);
			float c = Vector2.Dot(oc, oc) - other.Radius * other.Radius;

			float discriminant = b * b - 4 * a * c;
			if (discriminant >= 0f)
			{
				float sqrtDisc = MathF.Sqrt(discriminant);
				float t1 = (-b - sqrtDisc) / (2 * a);
				float t2 = (-b + sqrtDisc) / (2 * a);

				float t = float.MaxValue;
				if (t1 >= 0f && t1 <= totalLength) t = t1;
				if (t2 >= 0f && t2 <= totalLength && t2 < t) t = t2;

				if (t < float.MaxValue)
				{
					float distSqThis = t * t;
					if (distSqThis < minDistSq)
					{
						minDistSq = distSqThis;
						hasHit = true;
						Vector2 hitPos = from + rayDir * t;
						hitInfo.Wall = null;
						hitInfo.Body = other;
						hitInfo.HitPosition = hitPos;
						hitInfo.Distance = t;
					}
				}
			}
		}

		if (hasHit)
			return true;

		hitInfo.Distance = totalLength;
		hitInfo.HitPosition = to;
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

	public override void DrawImGui()
	{
		base.DrawImGui();

		ImGui.SeparatorText("Collision");
		ImGui.Text($"Wall count: {walls.Count}");
		ImGui.Text($"Body count: {bodies.Count}");
	}

	public override void DrawDebug()
	{
		base.DrawDebug();

		foreach (var i in walls)
		{
			Utils.DrawLineEx(i.From, i.To, i.Midpoint, i.Normal, Colors.RED);
		}
	}

	public override void Dispose()
	{
		base.Dispose();

		walls.Clear();
		bodies.Clear();
	}
}