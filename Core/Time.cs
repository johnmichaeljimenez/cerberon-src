namespace Main.Core;

public static class Time
{
	public static float DeltaTime => Raylib.GetFrameTime();
	public static float Alpha => _accumulator / FixedDeltaTime;

    public const float FixedDeltaTime = 1.0f / 60.0f;
	private static float _accumulator = 0.0f;

	public static void Update(Action<float> onUpdate)
	{
		float frameTime = Raylib.GetFrameTime();
		if (frameTime > 0.25f) frameTime = 0.25f;

		_accumulator += frameTime;

		while (_accumulator >= FixedDeltaTime)
		{
			onUpdate(FixedDeltaTime);
			_accumulator -= FixedDeltaTime;
		}
	}
}