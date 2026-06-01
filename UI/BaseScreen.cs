using Cerberon.Core;

namespace Cerberon.UI;

public abstract class BaseScreen : IDisposable
{
	public virtual string UIGroup => throw new NotImplementedException();
	protected readonly List<UIElement> elements = new();
	protected readonly Dictionary<string, UIElement> references = new(); //for lookup purposes, use list above for processing and/or guaranteed order

	protected UIElement hoveredElement { get; private set; }
	protected UIElement pressElement { get; private set; }
	protected readonly List<IDisposable> disposables = new();

	private readonly Dictionary<string, bool> visibilityGroups = new();

	private float inputStartDelay;

	public BaseScreen()
	{

	}

	public BaseScreen(object context = null)
	{

	}

	public virtual void UpdateElements(List<UIElement> elements)
	{
		hoveredElement = null;
		pressElement = null;

		this.elements.Clear();
		this.elements.AddRange(elements);

		this.references.Clear();
		foreach (var i in elements)
		{
			this.references[i.ID] = i;
		}

		UpdateVisibility();
	}

	public virtual void Draw()
	{
		foreach (var item in elements)
		{
			if (!item.CurrentVisibility)
				continue;

			item.Draw(hoveredElement == item);
		}
	}

	public virtual void OnEnter()
	{
		inputStartDelay = 0.4f;
		UpdateVisibility();
	}

	public void SetVisibility(UIElement e, bool visible)
	{
		e.Visible = visible;
		UpdateVisibility();
	}

	public void SetVisibility(string group, bool isVisible)
	{
		visibilityGroups[group] = isVisible;
		UpdateVisibility();
	}

	protected void UpdateVisibility()
	{
		foreach (var i in elements)
		{
			if (!i.Visible)
			{
				i.CurrentVisibility = false;
				continue;
			}

			var vis = true;
			foreach (var j in visibilityGroups)
			{
				if (!vis)
					break;

				foreach (var k in i.VisibilityGroups)
				{
					if (k != j.Key)
						continue;

					if (!j.Value)
					{
						vis = false;
						break;
					}
				}
			}

			i.CurrentVisibility = vis;
		}
	}

	public virtual void Update(float dt, float udt)
	{
		hoveredElement = null;

		if (inputStartDelay > 0)
		{
			inputStartDelay -= udt; //tiny QOL to avoid immediately triggering a newly-shown screen clickable element
			return;
		}

		for (int i = elements.Count - 1; i >= 0; i--)
		{
			var e = elements[i];
			if (!e.CurrentVisibility)
				continue;

			if (!e.Clickable)
				continue;

			var hover = !FadeHandler.Running && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), e.Rect);

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


		if (Raylib.IsMouseButtonReleased(0))
			pressElement = null;

		if (pressElement == null && Raylib.IsKeyPressed(KeyboardKey.Escape))
		{
			OnBack();
		}
	}

	public virtual void OnBack()
	{
		UIManager.Back();
	}

	public virtual void Dispose()
	{
		disposables.ForEach(p => p?.Dispose());
	}

	protected virtual void OnClick(UIElement e)
	{

	}
}