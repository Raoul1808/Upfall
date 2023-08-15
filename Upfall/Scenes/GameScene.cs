using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Upfall.Entities;

namespace Upfall.Scenes;

internal class GameScene : Scene
{
    private Player _player;
    private Tilemap _tilemap;
    
    public override void Load()
    {
        Assets.GetTexture("tileset");
        CanvasEffect = Assets.GetEffect("DynamicOneBit");
        if (CanvasEffect != null)
        {
            CanvasEffect.Parameters["BitColor1"].SetValue(Color.DarkRed.ToVector4());
            CanvasEffect.Parameters["BitColor2"].SetValue(Color.OrangeRed.ToVector4());
        }
    }

    public override void OnBecomeActive()
    {
        _tilemap = Tilemap.LoadFromFile("map.umd");
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
        _player = AddToScene<Player>();
        _player.Position = _tilemap.GetSpawnPos();
        _player.Velocity.X = 0;
    }

    public override void OnBecomeInactive()
    {
        RemoveFromScene(_player);  // Avoid duplicates when loading back into the scene
    }

    public override void Update(float dt)
    {
        bool quitting = InputManager.GetKeyPress(Keys.Escape);
        if (UpfallCommon.InEditor && quitting)
        {
            // We're playtesting, go back to the editor
            SceneManager.Change("Editor");
            return;  // Don't execute further
        }
        
        if (quitting)
        {
            // We're in-game, open pause menu to allow going back to main menu
            // TODO: make pause menu
            SceneManager.Change("Menu");
            return;  // Don't execute further
        }
        
        // TODO: add more gentle transitions
        if (_player.IsDead || _player.WonLevel)
        {
            SceneManager.Reload("Game");
            return;  // Don't execute further
        }
        
        // Prevent player from going out of screen
        if (_player.BoundingBox.Left < _tilemap.GetLeft())
        {
            _player.Position.X = _tilemap.GetLeft() + _player.BoundingBox.Width / 2f;
        }

        if (_player.BoundingBox.Right > _tilemap.GetRight())
        {
            _player.Position.X = _tilemap.GetRight() - _player.BoundingBox.Width / 2f;
        }
        
        // If player is OOB, kill him
        if (_player.BoundingBox.Top > _tilemap.GetBottom() || _player.BoundingBox.Bottom < _tilemap.GetTop())
        {
            _player.Kill();
        }
        
        base.Update(dt);
        _tilemap.SolveCollisions(_player);
    }

    public override void CanvasRender(SpriteBatch spriteBatch)
    {
        base.CanvasRender(spriteBatch);
        _tilemap.Render(spriteBatch);
    }
}
