using Main.Core;

namespace Main.UI;

public abstract class BaseScreen : IDisposable
{
	public virtual string UIGroup => throw new NotImplementedException();
	protected readonly List<UIElement> elements = new();

	protected UIElement hoveredElement { get; private set; }
	protected UIElement pressElement { get; private set; }

	public BaseScreen(object context = null)
	{

	}

	public virtual void UpdateElements(List<UIElement> elements)
	{
		hoveredElement = null;
		pressElement = null;

		this.elements.Clear();
		this.elements.AddRange(elements);
	}

	public virtual void Draw()
	{

	}

	public virtual void OnEnter()
	{

	}

	public virtual void Update(float dt, float udt)
	{
		hoveredElement = null;

		for (int i = elements.Count - 1; i >= 0; i--)
		{
			var e = elements[i];
			if (!e.Visible)
				continue;

			if (!e.Clickable)
				continue;

			var hover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), e.Rect);

			if (hover)
			{
				hoveredElement = e;
				
				if (Raylib.IsMouseButtonReleased(0))
				{
					//classic UX: only do onclick events on the button as long as the press and release events are on the same button
					//to cancel click, simply release it outside the button
					if (pressElement == e)
					{
						Log.Send($"click: {e.ID}");
						OnClick(e);
					}

					pressElement = null;
				}

				if (Raylib.IsMouseButtonPressed(0))
					pressElement = e;

				break;
			}
		}
	}

	public virtual void Dispose()
	{

	}

	protected virtual void OnClick(UIElement e)
	{

	}
}