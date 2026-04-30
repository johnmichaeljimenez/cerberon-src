using Main.Gameplay;

namespace Main.UI;

public class HUDScreen : BaseScreen
{
	public override string UIGroup => "HUD";
	
	private GameplayState gameplayState;

	public HUDScreen(object context) : base(context)
	{
		gameplayState = context as GameplayState;
	}
}