namespace Cerberon.Helpers;

public class EMA
{
	[JsonProperty]
	public float Alpha { get; private set; }

	private bool hasValue;
	private float ema;

	public EMA() { }

	public EMA(float alpha)
	{
		Alpha = alpha;
	}

	public void AddSample(float value)
	{
		if (hasValue)
		{
            ema = Alpha * value + (1f - Alpha) * ema;
		}
		else
		{
			ema = value; //seeder
			hasValue = true;
		}
	}

	[JsonIgnore]
	public float Current => ema;
	public void Reset() => hasValue = false;
}