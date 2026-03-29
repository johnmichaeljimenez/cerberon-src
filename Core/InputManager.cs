using System.Numerics;
using Raylib_cs;

namespace Main.Core;

public static class InputManager
{
    public static Vector2 GetVirtualMouse(float scale, Vector2 offset)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        return new Vector2((mouse.X - offset.X) / scale, (mouse.Y - offset.Y) / scale);
    }
}