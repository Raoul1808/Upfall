using System;
using System.Collections.Generic;
using System.IO;
using Brocco;
using Brocco.Basic;
using Brocco.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Upfall.Entities;

namespace Upfall;

public class Tilemap
{
    private int[,] _tiles;
    private int[,] _darkTiles;
    private int[,] _lightTiles;
    private Point _spawnPoint;
    private Point _endPoint;

    public Tilemap(int[,] tiles)
    {
        Size sz = new Size(tiles.GetLength(1), tiles.GetLength(0));
        _tiles = tiles;
        _darkTiles = new int[sz.Height, sz.Width];
        _lightTiles = new int[sz.Height, sz.Width];
    }

    public Tilemap(Size size)
    {
        _tiles = new int[size.Height, size.Width];
        _darkTiles = new int[size.Height, size.Width];
        _lightTiles = new int[size.Height, size.Width];
    }

    public void SetTile(Point pos, int tile)
    {
        _tiles[pos.Y, pos.X] = tile;
    }

    public int GetTile(int x, int y)
    {
        int tile = _tiles[y, x];
        if (tile == 0)
        {
            switch (UpfallCommon.CurrentWorldMode)
            {
                case WorldMode.Dark:
                    return _darkTiles[y, x];

                case WorldMode.Light:
                    return _lightTiles[y, x];
                
                case WorldMode.None:
                default:
                    return 0;
            }
        }

        return tile;
    }

    public Rectangle GetTileRect(Point pos) => GetTileRect(pos.X, pos.Y);

    public Rectangle GetTileRect(int x, int y) => new(x * TileSize, y * TileSize, TileSize, TileSize);
    
    public const int TileSize = 16;

    public void LoadFromFile(string path)
    {
        var reader = File.OpenText(path);
        var rows = int.Parse(reader.ReadLine() ?? "0");
        var cols = int.Parse(reader.ReadLine() ?? "0");
        _tiles = new int[rows, cols];
        for (int row = 0; row < _tiles.GetLength(0); row++)
        {
            var line = reader.ReadLine() ?? new string('0', cols);
            
            for (int col = 0; col < _tiles.GetLength(1); col++)
            {
                _tiles[row, col] = line[col];
            }
        }

        reader.Dispose();
    }
    
    public void SaveToFile(string path)
    {
        var writer = File.CreateText(path);
        writer.WriteLine(_tiles.GetLength(0));
        writer.WriteLine(_tiles.GetLength(1));
        for (int row = 0; row < _tiles.GetLength(0); row++)
        {
            for (int col = 0; col < _tiles.GetLength(1); col++)
            {
                writer.Write((char)_tiles[row, col]);
            }
            writer.WriteLine();
        }
        writer.Flush();
        writer.Dispose();
    }

    private string RectToStr(Rectangle rect) => $"{{Left: {rect.Left}, Right: {rect.Right}, Top: {rect.Top}, Bottom: {rect.Bottom}}}";
    
    public void SolveCollisions(TilemapEntity entity)
    {
        var pos = entity.Position / TileSize;
        int lx = Math.Max((int)pos.X - 1, 0);
        int ux = Math.Min((int)pos.X + 2, _tiles.GetLength(1) - 1);
        int ly = Math.Max((int)pos.Y - 1, 0);
        int uy = Math.Min((int)pos.Y + 2, _tiles.GetLength(0) - 1);

        var tileRects = new List<Tuple<float, Rectangle>>();

        for (int x = lx; x <= ux; x++)
        {
            for (int y = ly; y <= uy; y++)
            {
                var bbox = entity.BoundingBox;
                var tileRect = GetTileRect(x, y);
                int tile = GetTile(x, y);
                if (tile != 0 && bbox.Intersects(tileRect))
                {
                    tileRects.Add(new(BroccoMath.Distance(bbox.Center, tileRect.Center), tileRect));
                }
            }
        }

        if (tileRects.Count <= 0) return;  // No collision detected?
        
        tileRects.Sort((x, y) => x.Item1.CompareTo(y.Item1));

        foreach (var (_, tileRect) in tileRects)
        {
            // TODO: the collision code REALLY shouldn't be here, but I REALLY NEED to make progress in the game.
            if (!entity.BoundingBox.Intersects(tileRect)) continue;

            var depth = RectangleExtensions.GetIntersectionDepth(entity.BoundingBox, tileRect);

            float xDepthAbs = Math.Abs(depth.X);
            float yDepthAbs = Math.Abs(depth.Y);

            if (yDepthAbs < xDepthAbs)
            {
                if (entity.TouchedTopOf(tileRect))
                {
                    entity.Position.Y = tileRect.Top - entity.BoundingBox.Height / 2f;
                    entity.Velocity.Y = 0;
                    entity.OnTileTopTouched(tileRect);
                }

                if (entity.TouchedBottomOf(tileRect))
                {
                    entity.Position.Y = tileRect.Bottom + entity.BoundingBox.Height / 2f;
                    entity.Velocity.Y = 0;
                    entity.OnTileBottomTouched(tileRect);
                }
            }
            
            if (entity.TouchedLeftOf(tileRect))
            {
                entity.Position.X = tileRect.Left - entity.BoundingBox.Width / 2f;
                entity.Velocity.X = 0;
                entity.OnTileLeftTouched(tileRect);
            }
            
            if (entity.TouchedRightOf(tileRect))
            {
                entity.Position.X = tileRect.Right + entity.BoundingBox.Width / 2f;
                entity.Velocity.X = 0;
                entity.OnTileRightTouched(tileRect);
            }
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (int row = 0; row < _tiles.GetLength(0); row++)
        {
            for (int col = 0; col < _tiles.GetLength(1); col++)
            {
                int tile = _tiles[row, col];
                if (tile != 0)
                {
                    Color white = Color.White;
                    if (UpfallCommon.InEditor && UpfallCommon.CurrentWorldMode != WorldMode.None)
                        white *= 0.33f;
                    spriteBatch.Draw(Assets.Pixel, new Vector2(col * TileSize, row * TileSize), null, white, 0f, Vector2.Zero, new Vector2(TileSize), SpriteEffects.None, 0f);
                }
                switch (UpfallCommon.CurrentWorldMode)
                {
                    case WorldMode.Dark:
                        tile = _darkTiles[row, col];
                        if (tile != 0)
                            spriteBatch.Draw(Assets.Pixel, new Vector2(col * TileSize, row * TileSize), null, Color.White, 0f, Vector2.Zero, new Vector2(TileSize), SpriteEffects.None, 0f);
                        break;

                    case WorldMode.Light:
                        tile = _lightTiles[row, col];
                        if (tile != 0)
                            spriteBatch.Draw(Assets.Pixel, new Vector2(col * TileSize, row * TileSize), null, Color.White, 0f, Vector2.Zero, new Vector2(TileSize), SpriteEffects.None, 0f);
                        break;
                    
                    default:
                    case WorldMode.None:
                        break;
                }
            }
        }
    }
}
