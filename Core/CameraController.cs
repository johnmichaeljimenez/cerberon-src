namespace Main.Core;

public class CameraController
{
	public Camera2D Camera;

	private float followSpeed;
	private Vector2 followTarget;

	public CameraController(int virtualWidth, int virtualHeight)
	{
		Camera = new Camera2D
		{
			Target = Vector2.Zero,
			Offset = new Vector2(virtualWidth, virtualHeight) / 2f,
			Rotation = 0f,
			Zoom = Sprite.PIXELS_PER_UNIT / 4 //PIXELS_PER_UNIT = 1:1 sprite size on screen, 16 = default zoom level (higher value = closer zoom)
		};
	}

	public void Update(float dt)
	{
		AudioHandler.ListenerPosition = Camera.Target;

		//runs at fixed update (from Game) but still super smooth and accurate (unlike Cinemachine where simple camera teleporting needs complex code), but I don't know why tbh, it's like black magic for real
		if (followSpeed > 0)
			Camera.Target = Vector2.Lerp(Camera.Target, followTarget, followSpeed * dt);
	}

	public void Follow(Vector2 target, float speed = 0)
	{
		if (speed <= 0.001f)
		{
			Camera.Target = target;
			followTarget = target;
			return;
		}

		followSpeed = speed;
		followTarget = target;
	}
}