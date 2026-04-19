namespace Main.Core;

//i don't care about rebinding for now. and if I ever care of it, at least I just need to fix this script and worry about it in far future instead
public static class InputManager
{
    const float DEADZONE = 0.15f;

    public static bool HasGamepad => Raylib.IsGamepadAvailable(0);
    public static Vector2 MousePosition { get; private set; }
    public static Vector2 MouseWorldPosition { get; private set; }

    public static Vector2 Movement { get; private set; }
    public static bool ActionDown { get; private set; }
    public static bool ActionJustPressed { get; private set; }
    public static bool FlashlightJustPressed { get; private set; }
    public static bool Weapon1JustPressed { get; private set; }
    public static bool Weapon2JustPressed { get; private set; }
    public static bool ReloadJustPressed { get; private set; }

    public static void Update(float scale, Vector2 offset, Camera2D camera)
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

        ActionDown = Raylib.IsMouseButtonDown(MouseButton.Left);
        ActionJustPressed |= Raylib.IsMouseButtonPressed(MouseButton.Left); //i don't know what is this but it made the render loop -> fixed loop synchronization work

        FlashlightJustPressed |= Raylib.IsKeyPressed(KeyboardKey.F);
        Weapon1JustPressed |= Raylib.IsKeyPressed(KeyboardKey.One);
        Weapon2JustPressed |= Raylib.IsKeyPressed(KeyboardKey.Two);
        ReloadJustPressed |= Raylib.IsKeyPressed(KeyboardKey.R);
    }

    public static void LateUpdate()
    {
        //consume all pending active inputs here

        ActionJustPressed = false;
        FlashlightJustPressed = false;
        Weapon1JustPressed = false;
        Weapon2JustPressed = false;
        ReloadJustPressed = false;
    }

    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
}