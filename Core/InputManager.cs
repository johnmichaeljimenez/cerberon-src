namespace Main.Core;

public static class InputManager
{
    public static Vector2 MousePosition { get; private set; }

    internal static void Update(float scale, Vector2 offset)
    {
        Vector2 rawPos = Raylib.GetMousePosition();
        MousePosition = (rawPos - offset) / scale;
    }

    //i don't care about rebinding for now. and if I ever care of it, at least I just need to fix this script and worry about it in far future instead
    public static bool IsKeyDown(KeyboardKey key) => Raylib.IsKeyDown(key);
    public static bool IsKeyPressed(KeyboardKey key) => Raylib.IsKeyPressed(key);
    
    public static bool IsMouseButtonDown(MouseButton button) => Raylib.IsMouseButtonDown(button);
}