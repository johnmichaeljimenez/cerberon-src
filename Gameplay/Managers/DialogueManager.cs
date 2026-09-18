using Cerberon.Core;
using Cerberon.Helpers;

namespace Cerberon.Gameplay.Managers;

public class Dialogue
{
	public string Character { get; set; }
	public string Message { get; set; }

	public float Duration => MathF.Max(Message.Length * 0.1f, 2);
}

public class DialogueManager : BaseManager
{
	public readonly Signal<Dialogue> OnDialogueShow = new();

	private float timer;

	public bool Running { get; private set; }
	public Dialogue CurrentDialogue { get; private set; }

	public DialogueManager(GameplayState gameplayState) : base(gameplayState)
	{
	}

	public override void Init()
	{
		base.Init();
		CurrentDialogue = null;
	}

	public bool ShowDialogue(Dialogue dialogue)
	{
		if (CurrentDialogue != null)
			EndDialogue();

		CurrentDialogue = dialogue;
		timer = (CurrentDialogue.Message.Replace(" ", "").Length * 0.035f) + 1.0f;
		OnDialogueShow.Publish(CurrentDialogue);

		return true;
	}

	public void EndDialogue()
	{
		CurrentDialogue = null;
		OnDialogueShow.Publish(null);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (CurrentDialogue == null)
			return;

		if (Utils.Countdown(ref timer, dt))
		{
			EndDialogue();
		}
	}
}