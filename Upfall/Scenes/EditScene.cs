using System;
using Brocco;
using Brocco.Basic;
using Brocco.Input;
using Brocco.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall.Scenes;

public class EditScene : Scene
{
    private Tilemap _tilemap;
    private Size _tilemapSize;
    private Point _currentTilePos;
    
    public override void Load()
    {
        _tilemap = new Tilemap(_tilemapSize = new Size(40, 23));
    }

    public override void Update(float dt)
    {
        var pos = InputManager.GetCanvasMousePosition();
        int x = BroccoMath.Clamp((int)pos.X / Tilemap.TileSize, 0, _tilemapSize.Width - 1);
        int y = BroccoMath.Clamp((int)pos.Y / Tilemap.TileSize, 0, _tilemapSize.Height - 1);
        _currentTilePos = new Point(x, y);
        if (InputManager.GetClickDown(MouseButtons.Left))
            _tilemap.SetTile(_currentTilePos, 1);
        if (InputManager.GetClickDown(MouseButtons.Right))
            _tilemap.SetTile(_currentTilePos, 0);
    }

    public override void CanvasRender(SpriteBatch spriteBatch)
    {
        _tilemap.Render(spriteBatch);
        int s = Tilemap.TileSize;
        spriteBatch.Draw(Assets.Pixel, new Rectangle(_currentTilePos.X * s, _currentTilePos.Y * s, s, s), Color.DimGray * 0.5f);
    }
}
