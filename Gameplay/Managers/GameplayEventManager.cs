using System.Text;
using Main.Core;
using Main.Gameplay.Events;

namespace Main.Gameplay.Managers;

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
			sequences.Remove(existing);

		var sequence = new Sequence(gameplayState, id, commands);
		sequences.Add(sequence);

		return sequence;
	}

	public override void DrawImGui()
	{
		base.DrawImGui();

		if (ImGui.Button("Test"))
		{
			RunEvent("test", 
				new Print("Test"),
				new Fade(true),
				new Wait(1.0f),
				new PlayAudio("break", Vector2.Zero),
				new SpawnEnemy(Vector2.Zero),
				new Fade(false),
				new Wait(0.2f),
				new PlayAudio("break", Vector2.Zero),
				new SpawnEnemy(Vector2.Zero),
				new Wait(0.2f),
				new PlayAudio("break", Vector2.Zero),
				new SpawnEnemy(Vector2.Zero),
				new Wait(0.2f),
				new PlayAudio("break", Vector2.Zero),
				new SpawnEnemy(Vector2.Zero),
				new Print("OK!")
			);
		}
	}
}