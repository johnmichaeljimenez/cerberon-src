using Main.Core;
using Main.Helpers;

namespace Main.Gameplay.Managers;

public class Dialogue
{
	public string ID { get; set; }
	public string CharacterID { get; set; }
	public string Message { get; set; }

	public float Duration => MathF.Max(Message.Length * 0.1f, 2);
}

public class DialogueManager : BaseManager
{
	private const string DIALOGUE = "Assets/dialogue.tsv";
	private readonly Dictionary<string, List<Dialogue>> dialogues = new();
	public readonly Signal<Dialogue> OnDialogueShow = new();

	private string currentDialogueID;
	private int currentIndex;
	private float timer;

	public bool Running { get; private set; }
	public Dialogue CurrentDialogue { get; private set; }

	public DialogueManager(GameplayState gameplayState) : base(gameplayState)
	{
	}

	public override void Init()
	{
		base.Init();

		dialogues.Clear();
		foreach (var i in TsvParser.Parse<Dialogue>(AssetWatcher.Add(DIALOGUE, OnDialogueListChanged)))
		{
			if (!dialogues.ContainsKey(i.ID))
				dialogues[i.ID] = new();

			dialogues[i.ID].Add(i);
		}

		currentDialogueID = null;
		currentIndex = -1;
		CurrentDialogue = null;
	}

	private void OnDialogueListChanged(string content)
	{
		try
		{
			var temp = TsvParser.Parse<Dialogue>(content); //do not overwrite dialogue data if this is faulty
			dialogues.Clear();

			foreach (var i in temp)
			{
				if (!dialogues.ContainsKey(i.ID))
					dialogues[i.ID] = new();

				dialogues[i.ID].Add(i);
			}

			if (CurrentDialogue != null)
				EndDialogue();
		}
		catch (Exception ex)
		{
			Log.Send($"Dialogue TSV error: {ex.Message}");
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		AssetWatcher.Remove(DIALOGUE);
	}

	public bool ShowDialogue(string id)
	{
		if (!dialogues.ContainsKey(id) || dialogues[id].Count == 0)
			return false;

		currentDialogueID = id;
		currentIndex = 0;
		UpdateDialogue();

		return true;
	}

	public void EndDialogue()
	{
		CurrentDialogue = null;
		currentDialogueID = null;
		currentIndex = -1;
		OnDialogueShow.Publish(null);
	}

	public void Next()
	{
		currentIndex++;
		if (currentIndex >= dialogues[currentDialogueID].Count)
		{
			EndDialogue();
			return;
		}

		UpdateDialogue();
	}

	private void UpdateDialogue()
	{
		CurrentDialogue = dialogues[currentDialogueID][currentIndex];
		timer = CurrentDialogue.Duration;
		OnDialogueShow.Publish(CurrentDialogue);
	}

	public override void Update(float dt, float udt)
	{
		base.Update(dt, udt);

		if (CurrentDialogue == null)
			return;

		if (Utils.Countdown(ref timer, dt))
		{
			Next();
		}
	}
}