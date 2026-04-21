namespace Main.Core;

public class Animation //this is the "asset"
{
	public const int FRAMES_PER_SECOND = 8;
	public const float ANIMATION_FPS = 1.0f / (float)FRAMES_PER_SECOND;

	[JsonProperty]
	public string Name { get; private set; }
	[JsonProperty]
	public List<string> Frames { get; private set; } = new();
	[JsonProperty]
	public int LoopStartIndex { get; private set; } = -1; //-1 = non-looping, 0 = loop from start, >= 0 loop from any frame (useful for animations with "enter" frames)
}

public class Animator //this is the "instance" using those said "assets"
{
	public readonly Dictionary<string, Animation> Animations = new();
	public readonly Dictionary<Animation, List<Sprite>> SpriteFrames = new();

	public bool IsPlaying { get; private set; }
	private Animation currentAnimation;
	private int frameIndex;
	private float timer;

	public Animator(params string[] animationNames)
	{
		if (animationNames.Length == 0)
			return;

		Animations.Clear();
		foreach (var i in animationNames.Select(AssetManager.GetAnimation))
		{
			Animations.Add(i.Name, i);
		}

		SpriteFrames.Clear();
		foreach (var i in Animations.Values)
		{
			SpriteFrames[i] = new();

			Sprite prevSprite = AssetManager.MissingSprite;

			var n = 0;
			foreach (var j in i.Frames)
			{
				var sprite = string.IsNullOrEmpty(j) ? null : //null = invisible frame on purpose
															AssetManager.GetSprite(j);

				SpriteFrames[i].Add(sprite);
			}
		}
	}

	public void Update(float dt, float udt) //unscaled is for later use
	{
		if (!IsPlaying || currentAnimation == null || currentAnimation.Frames.Count == 0)
			return;

		timer += dt;
		if (timer >= Animation.ANIMATION_FPS)
		{
			timer = 0;
			frameIndex++;

			if (frameIndex >= currentAnimation.Frames.Count) //this loop runs at fixed 60hz (not fps), so it's guaranteed to be consistent anyway. dt = 1 / 60 constant
			{
				if (currentAnimation.LoopStartIndex >= 0)
				{
					frameIndex = currentAnimation.LoopStartIndex;
				}
				else
				{
					frameIndex = currentAnimation.Frames.Count - 1;
					IsPlaying = false;
				}
			}
		}
	}

	public Sprite GetFrameSprite()
	{
		if (currentAnimation == null || SpriteFrames[currentAnimation].Count == 0)
			return null; //intended to be null (not missing)

		return SpriteFrames[currentAnimation][frameIndex];
	}

	public void Play(string animationName, bool forceRestart = false)
	{
		if (!Animations.TryGetValue(animationName, out var anim))
			throw new ArgumentException($"Animation '{animationName}' not found.");

		if (!forceRestart && currentAnimation?.Name == animationName && IsPlaying)
			return;

		Reset();
		currentAnimation = anim;
		IsPlaying = true;
	}

	public void Stop()
	{
		Reset();
	}

	private void Reset()
	{
		timer = 0;
		IsPlaying = false;
		frameIndex = 0;
	}
}