namespace Main.Helpers;

public class ModifiedFloat
{
	private float _baseValue;
	public float BaseValue
	{
		get => _baseValue;
		set
		{
			_baseValue = value;
			UpdateValue();
		}
	}

	public float CurrentValue { get; private set; }
	private readonly Dictionary<string, float> modifiers = new();

	private float _modifierSum;

	public ModifiedFloat(float baseValue)
	{
		BaseValue = baseValue;
	}

	public void SetModifier(string id, float value)
	{
		if (modifiers.TryGetValue(id, out float oldValue))
			_modifierSum -= oldValue;

		modifiers[id] = value;
		_modifierSum += value;
		UpdateValue();
	}

	public float GetModifier(string id, float defaultValue = 0)
	{
		var value = 0f;
		if (!modifiers.TryGetValue(id, out value))
			value = defaultValue;

		return value;
	}

	public void RemoveModifier(string id)
	{
		if (!modifiers.TryGetValue(id, out float value))
			return;

		modifiers.Remove(id);
		_modifierSum -= value;
		UpdateValue();
	}

	private void UpdateValue()
	{
		CurrentValue = BaseValue + _modifierSum;
	}
}