namespace Main.Core;

[Serializable]
public class Animation //this is the "asset"
{
	public const int FRAMES_PER_SECOND = 16;
	public const float FRAME_RATE = 1.0f / (float)FRAMES_PER_SECOND;

	[JsonProperty]
	public string Name { get; private set; }
	[JsonProperty]
	public List<string> Frames { get; private set; } = new();
	[JsonProperty]
	public int LoopStartIndex { get; private set; } = -1; //-1 = non-looping, 0 = loop from start, >= 0 loop from any frame (useful for animations with "enter" frames)


	[JsonIgnore]
	public List<Sprite> Sprites { get; private set; } = new();

	public void Init()
	{
		var expandedFrames = new List<string>();
		foreach (var frameSpec in Frames)
		{
			if (frameSpec.EndsWith("*"))
			{
				string prefix = frameSpec.Substring(0, frameSpec.Length - 1);
				var matches = AssetManager.GetSpritesStartingWith(prefix);
				expandedFrames.AddRange(matches);
			}
			else
			{
				expandedFrames.Add(frameSpec ?? string.Empty);
			}
		}

		Frames = expandedFrames;

		Sprites.Clear();
		Sprites.AddRange(Frames.Select(AssetManager.GetSprite));

		var n = 0;
		foreach (var i in Frames)
		{
			if (string.IsNullOrEmpty(i))
			{
				Sprites[n] = null; //override null as invisible frame on purpose
			}

			n++;
		}
	}
}

public class Animator //this is the "instance" using those said "assets"
{
	public readonly Dictionary<string, Animation> Animations = new();

	public bool IsPlaying { get; private set; }
	private Animation currentAnimation;
	private string nextAnimationName;
	private int frameIndex;
	private float timer;

	public float NormalizedTime { get; private set; }

	public Action<string, int, float> OnFrameChanged { get; set; }
	public Action<string> OnAnimationBegin { get; set; }
	public Action<string> OnAnimationEnd { get; set; }

	private readonly Dictionary<string, int> priority = new();

	public bool IsPlayingOneShot { get; set; }

	public Animator(Dictionary<string, int> animationNames)
	{
		if (animationNames.Count == 0)
			return;

		Animations.Clear();
		priority.Clear();

		foreach (var i in animationNames.Keys.Select(AssetManager.GetAnimation))
		{
			Animations.Add(i.Name, i);
			priority[i.Name] = animationNames[i.Name];
		}

		Reset();
	}

	public Animator(params string[] animationNames)
	{
		if (animationNames.Length == 0)
			return;

		Animations.Clear();
		foreach (var i in animationNames.Select(AssetManager.GetAnimation))
		{
			Animations.Add(i.Name, i);
			priority[i.Name] = 0;
		}

		Reset();
	}

	//this is an optional (but very useful 90% of the games) DIY system called hierarchical state machine
	//it works by using priority level instead of transition lines to connect animation states, which is overkill for most of the time
	public void SetPriority(string animation, int value)
	{
		priority[animation] = value;
	}

	public void Update(float dt, float udt) //unscaled is for later use
	{
		if (!IsPlaying || currentAnimation == null || currentAnimation.Frames.Count == 0)
			return;

		timer += dt;
		if (timer >= Animation.FRAME_RATE)
		{
			timer = 0;
			frameIndex++;
			NormalizedTime = (float)frameIndex / (float)currentAnimation.Frames.Count;

			OnFrameChanged?.Invoke(currentAnimation.Name, frameIndex, NormalizedTime);

			if (frameIndex >= currentAnimation.Frames.Count) //this loop runs at fixed 60hz (not fps), so it's guaranteed to be consistent anyway. dt = 1 / 60 constant
			{
				if (currentAnimation.LoopStartIndex >= 0)
				{
					frameIndex = currentAnimation.LoopStartIndex;
					NormalizedTime = (float)frameIndex / (float)currentAnimation.Frames.Count;
				}
				else
				{
					OnAnimationEnd?.Invoke(currentAnimation.Name);
					frameIndex = currentAnimation.Frames.Count - 1;
					NormalizedTime = 1.0f;

					IsPlaying = false;
					IsPlayingOneShot = false;
				}
			}
		}

		if (!IsPlaying)
		{
			if (nextAnimationName != null)
			{
				Play(nextAnimationName, true, null, true); //intended ignore priority
			}
		}
	}

	public Sprite GetFrameSprite()
	{
		if (currentAnimation == null || currentAnimation.Sprites.Count == 0)
			return null; //intended to be null (not missing)

		return currentAnimation.Sprites[frameIndex];
	}

	public bool Play(string animationName, bool forceRestart = false, string nextAnimationName = null, bool ignorePriority = false) //nextAnimationName will be commonly used (ex. attack to idle) without coding massive amount of FSM handling
	{
		if (!Animations.TryGetValue(animationName, out var anim))
			throw new ArgumentException($"Animation '{animationName}' not found.");

		if (!forceRestart && currentAnimation?.Name == animationName && IsPlaying)
			return false;

		if (!ignorePriority && currentAnimation != null)
		{
			if (priority[currentAnimation.Name] > priority[animationName])
				return false;
		}

		Reset();
		currentAnimation = anim;
		this.nextAnimationName = nextAnimationName; //lazy check
		IsPlaying = true;
		NormalizedTime = 0f;

		IsPlayingOneShot = currentAnimation.LoopStartIndex < 0;
		OnAnimationBegin?.Invoke(animationName);

		return true;
	}

	public void Stop()
	{
		Reset();
	}

	private void Reset()
	{
		timer = 0;
		IsPlaying = false;
		IsPlayingOneShot = false;
		frameIndex = 0;
		nextAnimationName = null;
		NormalizedTime = 0;
	}
}