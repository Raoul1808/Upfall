using Brocco;
using Brocco.Basic;
using Brocco.Input;
using Brocco.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Upfall.Entities;

namespace Upfall.Scenes;

public class EditScene : Scene
{
    private Tilemap _tilemap;
    private Size _tilemapSize;
    private Point _currentTilePos;
    private Player _player;
    
    public override void Load()
    {
        ClearColor = Color.CornflowerBlue;
        _player = AddToScene<Player>();
        _tilemap = new Tilemap(_tilemapSize = new Size(40, 23));
        PauseUpdate = true;
        UpfallCommon.InEditor = true;
    }

    public override void OnBecomeActive()
    {
        UpfallCommon.CurrentWorldMode = WorldMode.None;
    }

    public override void Update(float dt)
    {
        if (InputManager.GetKeyPress(Keys.Tab))
            UpfallCommon.CycleWorldMode();

        _tilemap.SolveCollisions(_player);
        
        var pos = InputManager.GetCanvasMousePosition();
        int x = BroccoMath.Clamp((int)pos.X / Tilemap.TileSize, 0, _tilemapSize.Width - 1);
        int y = BroccoMath.Clamp((int)pos.Y / Tilemap.TileSize, 0, _tilemapSize.Height - 1);
        _currentTilePos = new Point(x, y);
        if (InputManager.GetClickDown(MouseButtons.Left))
            SetTile(_currentTilePos, 1);
        if (InputManager.GetClickDown(MouseButtons.Right))
            SetTile(_currentTilePos, 0);
        if (InputManager.GetClickDown(MouseButtons.Middle))
        {
            _tilemap.SetSpawn(_currentTilePos);
            _player.Position = _tilemap.GetSpawnPos();
        }

        if (InputManager.GetKeyDown(Keys.LeftControl))
        {
            if (InputManager.GetKeyPress(Keys.S))
            {
                _tilemap.SaveToFile("map.umd");
            }

            if (InputManager.GetKeyPress(Keys.O))
            {
                _tilemap = Tilemap.LoadFromFile("map.umd");
            }

            if (InputManager.GetKeyPress(Keys.T))
            {
                _tilemap.SaveToFile("map.umd");
                SceneManager.Change("Game");
            }
        }
    }

    private void SetTile(Point pos, int tile)
    {
        switch (UpfallCommon.CurrentWorldMode)
        {
            case WorldMode.None:
                _tilemap.SetCommonTile(pos, tile);
                break;

            case WorldMode.Dark:
                _tilemap.SetDarkTile(pos, tile);
                break;

            case WorldMode.Light:
                _tilemap.SetLightTile(pos, tile);
                break;
        }
    }

    public override void CanvasRender(SpriteBatch spriteBatch)
    {
        _tilemap.EditorRender(spriteBatch);
        _player.Render(spriteBatch);
        int s = Tilemap.TileSize;
        spriteBatch.Draw(Assets.Pixel, new Rectangle(_currentTilePos.X * s, _currentTilePos.Y * s, s, s), Color.DimGray * 0.5f);
    }
}
