using Cerberon.Core;

namespace Cerberon;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var game = new Game();
        game.Run();
        game.End();
    }
}