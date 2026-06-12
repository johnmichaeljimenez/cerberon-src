using Cerberon.Core;
using Cerberon.Gameplay.Entities;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class OptionsScreen : BaseScreen
{
	public override string UIGroup => "Options";
	private readonly Dictionary<string, string> hints = new()
	{
		{"btn-sfx", "Toggles general sound effects on/off"},
		{"btn-music", "Toggles music on/off"},
		{"btn-safe", "Changes enemy visuals and audio into safer version"},
	};

	public OptionsScreen(object context) : base(context)
	{

	}

	public override void UpdateElements(List<UIElement> elements)
	{
		base.UpdateElements(elements);

		references["hint-text"].CurrentVisibility = false;
		UpdateState();
	}

	private void UpdateState()
	{
		references["btn-sfx"].Text = $"Sound Effects: {(AudioHandler.SoundEnabled ? "ON" : "OFF")}";
		references["btn-music"].Text = $"Music: {(AudioHandler.MusicEnabled ? "ON" : "OFF")}";
		references["btn-safe"].Text = $"Safe Mode: {(AudioHandler.MusicEnabled ? "ON" : "OFF")}";
	}

	protected override void OnClick(UIElement e)
	{
		base.OnClick(e);

		switch (e.ID)
		{
			case "btn-sfx":
				AudioHandler.SoundEnabled = !AudioHandler.SoundEnabled;
				UpdateState();
				break;
			case "btn-music":
				AudioHandler.MusicEnabled = !AudioHandler.MusicEnabled;
				UpdateState();
				break;
			case "btn-safe":
				EnemyEntity.SafeMode = !EnemyEntity.SafeMode;
				UpdateState();
				break;
		}
	}

	public override void Draw()
	{
		var hint = references["hint-text"];
		if (hoveredElement != null)
		{
			hint.Text = hints[hoveredElement.ID];
			hint.CurrentVisibility = true;
		}
		else
		{
			hint.CurrentVisibility = false;
		}

		base.Draw();
	}
}