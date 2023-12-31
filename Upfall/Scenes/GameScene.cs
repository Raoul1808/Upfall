using Brocco;
using Brocco.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Upfall.Entities;

namespace Upfall.Scenes;

internal class GameScene : Scene
{
    private Player _player;
    private Tilemap _tilemap;

    private bool _waitForEndParticles = false;
    
    public override void Load()
    {
        Assets.GetTexture("tileset");
        ScreenEffect = Assets.GetEffect("DynamicOneBit");
        BlendState = BlendState.Additive;
    }

    public override void OnBecomeActive()
    {
        _waitForEndParticles = false;
        UpfallCommon.OnWorldChange += SetCircleAnim;
        _tilemap = UpfallCommon.Playtesting ? Tilemap.LoadFromFile(EditScene.TilemapToLoad) : UpfallCommon.LoadCurrentLevel();
        PaletteSystem.SetPalette(_tilemap.LevelPalette, UpfallCommon.Playtesting ? 0f : 1f);
        UpfallCommon.CurrentWorldMode = WorldMode.Dark;
        _player = AddToScene<Player>();
        _player.Position = _tilemap.GetSpawnPos();
        _player.Velocity.X = 0;
    }

    public override void OnBecomeInactive()
    {
        RemoveFromScene(_player);  // Avoid duplicates when loading back into the scene
        UpfallCommon.OnWorldChange -= SetCircleAnim;
    }

    private void SetCircleAnim(WorldMode oldMode, WorldMode newMode)
    {
        switch (newMode)
        {
            case WorldMode.Dark:
                ShaderEffectSystem.SetCircleRadiusAnim(200f, 0f);
                break;
            
            case WorldMode.Light:
                ShaderEffectSystem.SetCircleRadiusAnim(0f, 200f);
                break;
        }
    }

    public override void Update(float dt)
    {
        AnimationHelper.UpdateFrames();
        ParticleSystem.UpdateParticles(dt);
        bool quitting = InputManager.GetKeyPress(Keys.Escape) || InputManager.GetButtonPress(Buttons.Start);
        if (UpfallCommon.Playtesting && quitting)
        {
            // We're playtesting, go back to the editor
            SceneManager.Change("Editor");
            return;  // Don't execute further
        }
        
        if (quitting)
        {
            // We're in-game, open pause menu to allow going back to main menu
            // TODO: make pause menu
            UpfallCommon.LeaveLevelSet();
            SceneManager.Change("Menu");
            return;  // Don't execute further
        }

        if (_player.IsDead && _waitForEndParticles && ParticleSystem.DeathParticlesDone())
        {
            SceneManager.Change("Game");
            return;
        }

        if (_player.WonLevel && _waitForEndParticles && ParticleSystem.DeathParticlesDone())
        {
            if (!UpfallCommon.Playtesting)
            {
                if (UpfallCommon.HasNextLevel())
                    UpfallCommon.NextLevel();
                else
                {
                    SceneManager.Change("Menu");
                    return;
                }
            }

            SceneManager.Change("Game");
            return;
        }

        if (!_waitForEndParticles && (_player.IsDead || _player.WonLevel))
        {
            _waitForEndParticles = true;
            UpfallCommon.CurrentWorldMode = WorldMode.Dark;
            return;  // Don't execute further
        }

        if (!_player.IsDead)
        {
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
            if ((_player.BoundingBox.Top > _tilemap.GetBottom() && UpfallCommon.CurrentWorldMode == WorldMode.Dark) ||  // Player below the map and is falling down
                (_player.BoundingBox.Bottom < _tilemap.GetTop() && UpfallCommon.CurrentWorldMode == WorldMode.Light))  // Player above the the map and is falling up
            {
                _player.Kill();
            }
        }


        base.Update(dt);
        
        if (!_player.IsDead && !_player.WonLevel)
            _tilemap.SolveCollisions(_player);
        
        ShaderEffectSystem.SetCircleCanvasPos(_player.Position);
    }

    public override void CanvasRender(SpriteBatch spriteBatch)
    {
        base.CanvasRender(spriteBatch);
        _tilemap.Render(spriteBatch);
        ParticleSystem.RenderParticles(spriteBatch);
    }

    public override void ScreenRender(SpriteBatch spriteBatch)
    {
        if (NotificationSystem.CanRenderName())
            NotificationSystem.RenderName(spriteBatch);
    }
}
