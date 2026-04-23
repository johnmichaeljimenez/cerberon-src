namespace Main.Helpers;

public class EMA
{
	private readonly float _alpha;
	private bool hasValue;
	private float ema;

	public EMA(float alpha)
	{
		_alpha = alpha;
	}

	public void AddSample(float value)
	{
		if (hasValue)
		{
			ema = _alpha * value + (1f - _alpha) * ema;
		}
		else
		{
			ema = value; //seeder
			hasValue = true;
		}
	}

	public float Current => ema;
}