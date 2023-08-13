using Brocco;
using Upfall.Entities;

namespace Upfall.Scenes;

internal class GameScene : Scene
{
    public override void Load()
    {
        AddToScene<Player>();
    }
}
