using System.Security.Cryptography;
using Cerberon.Core;
using Cerberon.Effects;
using Cerberon.Gameplay.Managers;
using Cerberon.Helpers;
using static Cerberon.Gameplay.Managers.WaypointManager;

namespace Cerberon.Gameplay.Level;

public class NodeData
{
	private readonly Dictionary<int, List<Node>> nodeExposureLevels = new();

	public NodeData(GameplayState state)
	{
		//gather raw data
		var nodes = state.GetManager<WaypointManager>().Nodes;
		foreach (var i in nodes)
		{
			var group = i.Exposure > 0 ? (int)MathF.Round(1 / i.Exposure) : 0;
			if (!nodeExposureLevels.ContainsKey(group))
				nodeExposureLevels[group] = new();

			nodeExposureLevels[group].Add(i);
		}
	}

	public Node GetExposedNode(Vector2 position, float minRange, float maxRange, Func<Vector2, Vector2, bool> linecast = null)
	{
		var key = nodeExposureLevels.Keys.Max();
		return GetNode(position, key, minRange, maxRange, linecast);
	}

	public Node GetHiddenNode(Vector2 position, float minRange, float maxRange, Func<Vector2, Vector2, bool> linecast = null)
	{
		var key = nodeExposureLevels.Keys.Min();
		return GetNode(position, key, minRange, maxRange, linecast);
	}

	private Node GetNode(Vector2 position, int key, float minRange, float maxRange, Func<Vector2, Vector2, bool> linecast = null)
	{
		var nodes = nodeExposureLevels[key].ToList();
		nodes.Shuffle();

		foreach (var i in nodes)
		{
			var d = (position - i.Position).LengthSquared();
			if (d > maxRange * maxRange || d < minRange * minRange)
				continue;

			if (linecast != null && !linecast(position, i.Position))	//return only nodes invisible from line of sight
				continue;

			return i;
		}

		Log.Send("Cannot find valid node");
		return nodes.OrderBy(p => (p.Position - position).LengthSquared()).Last(); //failsafe
	}
}