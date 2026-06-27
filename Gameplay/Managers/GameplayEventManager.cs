using System.Text;
using Cerberon.Core;
using Cerberon.Gameplay.Events;

namespace Cerberon.Gameplay.Managers;

public class GameplayEventManager : BaseManager
{
	private readonly List<Sequence> sequences = new();

	public GameplayEventManager(GameplayState gameplayState) : base(gameplayState)
	{

	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (PauseHandler.IsPaused || sequences.Count == 0)
			return;

		for (int i = sequences.Count - 1; i >= 0; i--)
		{
			var sequence = sequences[i];
			if (!sequence.IsRunning)
				sequences.RemoveAt(i);
			else
				sequence.Update(dt);
		}
	}

	public Sequence RunEvent(string id, params BaseCommand[] commands)
	{
		if (commands.Length == 0)
			throw new InvalidDataException($"Empty command list for: {id}");

		var existing = sequences.FirstOrDefault(p => p.ID == id);
		if (existing != null)
		{
			existing.Stop();
			sequences.Remove(existing);
		}

		var sequence = new Sequence(gameplayState, id, commands);
		sequences.Add(sequence);

		return sequence;
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		if (ImGui.Button("Test"))
		{
			RunEvent("power",
				new PlayAudio("phone", null, true),
				new Wait(0.1f),
				new PlayAudio("phone", null, true),
				new ShowDialogue("power-1", false),
				new Wait(0.5f)
			);
		}

		if (ImGui.Button("Test2"))
		{
			RunEvent("power",
				new SetLightGroupState("Main", false),
				new Wait(0.1f),
				new SetLightGroupState("Main", true),
				new Wait(0.1f),
				new SetLightGroupState("Main", false),
				new Wait(0.1f),
				new SetLightGroupState("Main", true),
				new Wait(0.1f),
				new SetLightGroupState("Main", false),
				new ShowDialogue("power-2", false)
			);
		}
	}
}