using Main.Effects;
using Main.Gameplay.Managers;
using static Main.Gameplay.Managers.WaypointManager;

namespace Main.Gameplay.Level;

public struct NodeProperty
{
	public float OutdoorLight;
}

public class NodeData
{
	public readonly Dictionary<Node, NodeProperty> Nodes = new();

	public NodeData(GameplayState state)
	{
		//gather raw data
		var nodes = state.GetManager<WaypointManager>().Nodes;
		foreach (var i in nodes)
		{
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
}