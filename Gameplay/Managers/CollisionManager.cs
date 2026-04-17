//DISCLAIMER: This entire CollisionManager.cs is LLM-generated. Will remake this from scratch once I will actuall use it and once I tested this one to be working
//but basically I just want it to have 3 public functions for collide-and-slide for AABB only, as well as circle (character) to line (level props), and also line-line (simple raycast)

namespace Main.Gameplay;

public abstract class ColliderBody
{
	public bool IsEnabled = true;
	public Vector2 Position;
}

public class ColliderCircle : ColliderBody
{
	public float Radius;
}

public class ColliderBox : ColliderBody
{
	public Vector2 Size;
	public float Rotation; // degrees
	public readonly List<Edge> Edges = new List<Edge>(4);

	/// <summary>
	/// Creates a box collider and immediately builds its edges (outward normals).
	/// Call UpdateEdges() manually if you ever change Position/Size/Rotation later.
	/// </summary>
	public ColliderBox(Vector2 position, Vector2 size, float rotation = 0f)
	{
		Position = position;
		Size = size;
		Rotation = rotation;
		UpdateEdges();
	}

	public void UpdateEdges()
	{
		Edges.Clear();

		float rotRad = Rotation * MathF.PI / 180f;
		float c = MathF.Cos(rotRad);
		float s = MathF.Sin(rotRad);
		Vector2 half = Size * 0.5f;

		// Local unrotated corners (CCW order)
		Vector2[] localCorners = new Vector2[4]
		{
			new Vector2(-half.X, -half.Y),
			new Vector2( half.X, -half.Y),
			new Vector2( half.X,  half.Y),
			new Vector2(-half.X,  half.Y)
		};

		// Rotate + translate to world space
		Vector2[] worldCorners = new Vector2[4];
		for (int i = 0; i < 4; i++)
		{
			Vector2 local = localCorners[i];
			Vector2 rotated = new Vector2(
				local.X * c - local.Y * s,
				local.X * s + local.Y * c
			);
			worldCorners[i] = Position + rotated;
		}

		// Create 4 edges (CCW winding)
		for (int i = 0; i < 4; i++)
		{
			Vector2 from = worldCorners[i];
			Vector2 to = worldCorners[(i + 1) % 4];
			Vector2 dir = to - from;

			// Outward normal for CCW polygon (rotate dir 90° CW)
			Vector2 normal = Vector2.Normalize(new Vector2(dir.Y, -dir.X));

			Edges.Add(new Edge { From = from, To = to, Normal = normal });
		}
	}
}

public class Edge
{
	public Vector2 From, To;
	public Vector2 Normal;
}

public class CollisionManager
{
	public CollisionManager() { }

	/// <summary>
	/// Simple overlap detector (no resolution). Returns true if the circle overlaps any edge.
	/// </summary>
	public bool GetCollision(ColliderCircle from, List<Edge> edges)
	{
		if (!from.IsEnabled) return false;

		foreach (var edge in edges)
		{
			if (CircleIntersectsEdge(from, edge))
				return true;
		}
		return false;
	}

	/// <summary>
	/// Full collision resolution + sliding.
	/// - Modifies from.Position to push the circle out of penetration.
	/// - Modifies ref vel to remove the component toward any colliding surface (true sliding).
	/// - Iteratively resolves multiple edges in a single frame so concave corners work correctly
	///   (no sticking, no tunneling through 90° inner corners).
	/// </summary>
	public bool GetCollision(ColliderCircle from, List<Edge> edges, ref Vector2 vel)
	{
		if (!from.IsEnabled) return false;

		bool collided = false;
		const int maxIterations = 5; // plenty for any realistic corner situation

		for (int iter = 0; iter < maxIterations; iter++)
		{
			bool resolvedThisPass = false;

			foreach (var edge in edges)
			{
				if (CircleEdgeCollisionDetailed(from, edge, out Vector2 normal, out float penetration))
				{
					// Depenetrate + tiny bias to prevent future sticking
					from.Position += normal * (penetration + 0.001f);

					// Slide velocity (remove normal component only if moving into the wall)
					float dot = Vector2.Dot(vel, normal);
					if (dot < 0f)
					{
						vel -= normal * dot;
					}

					collided = true;
					resolvedThisPass = true;
				}
			}

			// No more collisions this pass → we are done
			if (!resolvedThisPass)
				break;
		}

		return collided;
	}

	// -----------------------------------------------------------------------
	// Private helpers (pure geometry, works for both open edges and closed shapes)
	// -----------------------------------------------------------------------

	private bool CircleIntersectsEdge(ColliderCircle circle, Edge edge)
	{
		Vector2 closest = GetClosestPointOnSegment(circle.Position, edge.From, edge.To);
		float distSq = Vector2.DistanceSquared(circle.Position, closest);
		return distSq <= circle.Radius * circle.Radius + 0.0001f;
	}

	private bool CircleEdgeCollisionDetailed(ColliderCircle circle, Edge edge,
		out Vector2 normal, out float penetration)
	{
		Vector2 closest = GetClosestPointOnSegment(circle.Position, edge.From, edge.To);
		Vector2 toCenter = circle.Position - closest;
		float dist = toCenter.Length();

		if (dist < circle.Radius && dist > 0.00001f)
		{
			normal = toCenter / dist;           // radial normal from wall → circle
			penetration = circle.Radius - dist;
			return true;
		}

		normal = Vector2.Zero;
		penetration = 0f;
		return false;
	}

	private Vector2 GetClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
	{
		Vector2 ab = b - a;
		float len2 = ab.LengthSquared();
		if (len2 == 0f) return a;

		float t = Vector2.Dot(p - a, ab) / len2;
		t = Math.Clamp(t, 0f, 1f);
		return a + t * ab;
	}
}