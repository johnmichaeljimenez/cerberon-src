namespace Main.Core;

public static class Time
{
	public static float CurrentTime { get; private set; }
	public static float DeltaTime => Raylib.GetFrameTime();
	public static float UnscaledDeltaTime => PauseHandler.IsPaused ? 0 : DeltaTime;
	public static float Alpha => _accumulator / FixedDeltaTime;

	public static float FixedDeltaTime => PauseHandler.IsPaused ? 0 : UnscaledFixedDeltaTime;
	public const float UnscaledFixedDeltaTime = 1.0f / 60.0f;
	private static float _accumulator = 0.0f;

	public static void Update(Action<float, float> onUpdate) //fixed, unscaled fixed
	{
		CurrentTime += UnscaledDeltaTime;

		float frameTime = UnscaledDeltaTime;
		if (frameTime > 0.25f) frameTime = 0.25f;

		_accumulator += frameTime;

		while (_accumulator >= UnscaledFixedDeltaTime)
		{
			onUpdate(FixedDeltaTime, UnscaledFixedDeltaTime);
			_accumulator -= UnscaledFixedDeltaTime;
		}
	}
}