using Raylib_cs;
using System.Numerics;

namespace Main.Core;

public class Game
{
    private const int VirtualWidth = 800;
    private const int VirtualHeight = 450;
    private RenderTexture2D _target = Raylib.LoadRenderTexture(VirtualWidth, VirtualHeight);

    public void Run()
    {
        while (!Raylib.WindowShouldClose())
        {
            float scale = RenderingManager.GetScale(VirtualWidth, VirtualHeight);
            Vector2 offset = RenderingManager.GetOffset(VirtualWidth, VirtualHeight, scale);
            Vector2 virtualMouse = InputManager.GetVirtualMouse(scale, offset);

            Raylib.BeginTextureMode(_target);
            Raylib.ClearBackground(Color.RayWhite);
            Raylib.DrawCircleV(virtualMouse, 10, Color.Blue);
            Raylib.DrawText("Centered Content", 20, 20, 20, Color.DarkGray);
            Raylib.DrawRectangle(100, 100, 200, 200, Color.Red);
            Raylib.EndTextureMode();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            RenderingManager.DrawToScreen(_target, scale, offset, VirtualWidth, VirtualHeight);
            Raylib.EndDrawing();
        }
        Raylib.UnloadRenderTexture(_target);
    }
}