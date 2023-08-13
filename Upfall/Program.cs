using Brocco;
using Brocco.Basic;
using Microsoft.Xna.Framework;
using Upfall.Scenes;

namespace Upfall;

internal class Program
{
    public static void Main()
    {
        var game = new BroccoGame(new BroccoGameSettings
        {
            CanvasSize = new Size(640, 360),
            Resolution = new Size(1280, 720),
            ClearColor = Color.CornflowerBlue,
            ShowMouse = true,
        });

        SceneManager.Add("Edit Scene", new EditScene());
        SceneManager.Add("Game Scene", new GameScene());

        game.Run();
    }
}
