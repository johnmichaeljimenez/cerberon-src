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
			Zoom = Sprite.PIXELS_PER_UNIT / 2 //64 = 1:1 sprite size on screen, 32 = half size as default zoom level (higher value = closer zoom)
		};
	}

	public void Update(float dt)
	{
		if (followSpeed > 0)
			Camera.Target = Vector2.Lerp(Camera.Target, followTarget, followSpeed * dt); //super smooth and accurate follow even in fixed update (unlike Cinemachine struggles)
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