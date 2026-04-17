namespace Main.Core;

public class CameraController
{
	public Camera2D Camera;

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
}