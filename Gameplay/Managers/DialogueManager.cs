using Main.Core;

namespace Main.Gameplay.Managers;

public class Dialogue
{
	public string ID { get; set; }
	public string CharacterID { get; set; }
	public string Message { get; set; }
}

public class DialogueManager : BaseManager
{
	private const string DIALOGUE = "Assets/dialogue.tsv";
	private readonly Dictionary<string, List<Dialogue>> dialogues = new();
	public readonly Signal<Dialogue> OnDialogueShow = new();

	private string currentDialogueID;
	private int currentIndex;

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
		if (currentIndex >= dialogues[currentDialogueID].Count)
		{
			EndDialogue();
			return;
		}

		currentIndex++;
		UpdateDialogue();
	}

	private void UpdateDialogue()
	{
		CurrentDialogue = dialogues[currentDialogueID][currentIndex];
		OnDialogueShow.Publish(CurrentDialogue);
	}
}