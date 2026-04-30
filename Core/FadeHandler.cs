using Main.Helpers;

namespace Main.Core;

public static class FadeHandler
{
	private const float FadeDuration = 0.3f;

	public static bool IsFading => isFading;

	private static float fadeTarget;
	private static float fadeTimer;
	private static bool isFading;
	private static Action fadeAction;
	private static bool toFadeOut;

	public static void FadeIn(Action onEnd = null, bool queueFadeOut = false)
	{
		isFading = true;
		fadeTimer = 0;
		fadeTarget = 1;
		fadeAction = onEnd;
		toFadeOut = queueFadeOut;
		RenderingManager.SetFade(0);
	}

	public static void FadeOut(Action onEnd = null)
	{
		isFading = true;
		fadeTimer = 0;
		fadeTarget = 0;
		fadeAction = onEnd;
		RenderingManager.SetFade(1);
	}

	public static void Update()
	{
		if (!isFading)
			return;

		if (fadeTimer < FadeDuration)
		{
			fadeTimer += Time.UnscaledDeltaTime;
			if (fadeTimer >= FadeDuration)
			{
				if (fadeTarget < 1)
				{
					RenderingManager.SetFade(0);
					isFading = false;
				}

				fadeAction?.Invoke();

				if (toFadeOut)
				{
					toFadeOut = false;
					FadeOut();
				}
			}
		}
	}

	public static void Draw()
	{
		if (!isFading)
			return;

		var t = Raymath.Lerp(fadeTarget > 0 ? 0 : 1, fadeTarget > 0 ? fadeTarget : 0f, fadeTimer / FadeDuration);
		var fadeColor = new Color(0f, 0f, 0f, t);

		RenderingManager.SetFade(t);
		Raylib.DrawRectangle(-10, -10, Raylib.GetScreenWidth() + 10, Raylib.GetScreenHeight() + 10, fadeColor); //for UI stuff (ingame fade is handled by shader)
	}
}