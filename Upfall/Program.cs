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
            CanvasSize = new Size(320, 180),
            Resolution = new Size(1280, 720),
            ClearColor = Color.CornflowerBlue,
        });
        
        SceneManager.Add("Game Scene", new GameScene());

        game.Run();
    }
}
