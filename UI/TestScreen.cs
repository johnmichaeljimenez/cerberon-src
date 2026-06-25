using Cerberon.Core;
using Cerberon.Gameplay;
using Cerberon.Helpers;

namespace Cerberon.UI;

public class TestScreen : BaseScreen
{
	public override string UIGroup => uiGroup;
	private string uiGroup;

	public TestScreen(object context) : base(context)
	{
		uiGroup = (string)context;
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}
}