using Brocco;
using Upfall.Entities;

namespace Upfall.Scenes;

internal class GameScene : Scene
{
    private Player _player;
    private Platform _platform;
    
    public override void Load()
    {
        _player = AddToScene<Player>();
        _platform = AddToScene<Platform>();
    }

    public override void Update(float dt)
    {
        base.Update(dt);
        _player.ResolveCollision(_platform);
    }
}
