namespace Main.Core;

public static class InputManager
{
    const float DEADZONE = 0.15f;

    public static bool HasGamepad => Raylib.IsGamepadAvailable(0);
    public static Vector2 MousePosition { get; private set; }
    public static Vector2 MouseWorldPosition { get; private set; }
    public static Vector2 Movement { get; private set; }

    internal static void Update(float scale, Vector2 offset, Camera2D camera)
    {
        Vector2 rawPos = Raylib.GetMousePosition();
        MousePosition = (rawPos - offset) / scale;

        MouseWorldPosition = Raylib.GetScreenToWorld2D(MousePosition, camera); //i dont care if there's "circular dependency" here if they are just about Game.cs and InputManager.cs and the benefit is 100% immediate usability in gameplay

        float gpX = HasGamepad ? Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftX) : 0f;
        float gpY = HasGamepad ? Raylib.GetGamepadAxisMovement(0, GamepadAxis.LeftY) : 0f;

        gpX = MathF.Abs(gpX) < DEADZONE ? 0f : gpX;
        gpY = MathF.Abs(gpY) < DEADZONE ? 0f : gpY;

        Vector2 dir = new Vector2
        {
            X = (IsKeyDown(KeyboardKey.A) ? -1 : IsKeyDown(KeyboardKey.D) ? 1 : 0) + gpX,
            Y = (IsKeyDown(KeyboardKey.W) ? -1 : IsKeyDown(KeyboardKey.S) ? 1 : 0) + gpY
        };

        Movement = (dir.X != 0 || dir.Y != 0) ? Raymath.Vector2Normalize(dir) : dir;
    }

    //i don't care about rebinding for now. and if I ever care of it, at least I just need to fix this script and worry about it in far future instead
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);

    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
}