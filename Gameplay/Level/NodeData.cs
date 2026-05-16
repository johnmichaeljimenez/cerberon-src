using System.Security.Cryptography;
using Main.Effects;
using Main.Gameplay.Managers;
using Main.Helpers;
using static Main.Gameplay.Managers.WaypointManager;

namespace Main.Gameplay.Level;

public struct NodeProperty
{
	public float OutdoorLight;
}

public class NodeData
{
	public readonly Dictionary<Node, NodeProperty> Nodes = new();
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

			Nodes[i] = new()
			{
				OutdoorLight = LightingSystem.GetOutdoorLightFactor(i.Position)
			};
		}

		//blur the outdoor light data
		var newLightValues = new Dictionary<Node, float>();
		foreach (var node in Nodes.Keys)
		{
			float sum = Nodes[node].OutdoorLight;
			int count = 1;

			foreach (var connectedNode in node.Connections)
			{
				if (Nodes.ContainsKey(connectedNode))
				{
					sum += Nodes[connectedNode].OutdoorLight;
					count++;
				}
			}

			newLightValues[node] = sum / count;
		}

		foreach (var node in newLightValues.Keys)
		{
			Nodes[node] = new()
			{
				OutdoorLight = newLightValues[node]
			};
		}
	}

	public bool IsOutdoor(Node n, bool withOutdoorLight)
	{
		LightingSystem.AmbientLightColor.GetHSV(out var h, out var s, out var amb);
		return (!withOutdoorLight || amb >= 0.2f) && Nodes[n].OutdoorLight >= 0.5f; //outdoor enough and outdoor light is bright enough
	}

	public Node GetExposedNode(Vector2 position, float minRange, float maxRange)
	{
		var key = nodeExposureLevels.Keys.Max();
		return GetNode(position, key, minRange, maxRange);
	}

	public Node GetHiddenNode(Vector2 position, float minRange, float maxRange)
	{
		var key = nodeExposureLevels.Keys.Min();
		return GetNode(position, key, minRange, maxRange);
	}

	private Node GetNode(Vector2 position, int key, float minRange, float maxRange)
	{
		var nodes = nodeExposureLevels[key].ToList();
		nodes.Shuffle();

		foreach (var i in nodes)
		{
			var d = (position - i.Position).LengthSquared();
			if (d > maxRange * maxRange || d < minRange * minRange)
				continue;

			return i;
		}

		return nodes[nodes.Count - 1]; //failsafe
	}
}