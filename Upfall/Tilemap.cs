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
    private Size _tilemapSize;
    private Point _spawnPoint;
    private Point _endPoint;

    public Vector2 HalfTile => new(TileSize / 2f);

    private Tilemap()
    {
    }
    
    public Tilemap(Size size)
    {
        _tilemapSize = size;
        _tiles = new int[size.Height, size.Width];
        _darkTiles = new int[size.Height, size.Width];
        _lightTiles = new int[size.Height, size.Width];
    }

    public void SetCommonTile(Point pos, int tile)
    {
        _tiles[pos.Y, pos.X] = tile;
    }

    public void SetDarkTile(Point pos, int tile)
    {
        _darkTiles[pos.Y, pos.X] = tile;
    }

    public void SetLightTile(Point pos, int tile)
    {
        _lightTiles[pos.Y, pos.X] = tile;
    }

    public void SetSpawn(Point pos)
    {
        _spawnPoint = pos;
    }

    public void SetExit(Point pos)
    {
        _endPoint = pos;
    }

    public int GetTileCommon(int x, int y) => _tiles[y, x];

    public int GetTileDark(int x, int y) => _darkTiles[y, x];

    public int GetTileLight(int x, int y) => _lightTiles[y, x];

    public int GetTile(int x, int y)
    {
        int tile = GetTileCommon(x, y);
        if (tile == 0)
        {
            switch (UpfallCommon.CurrentWorldMode)
            {
                case WorldMode.Dark:
                    return GetTileDark(x, y);

                case WorldMode.Light:
                    return GetTileLight(x, y);
                
                case WorldMode.None:
                default:
                    return 0;
            }
        }

        return tile;
    }

    public Vector2 GetSpawnPos() => GetTilePos(_spawnPoint).ToVector2() + HalfTile;

    public Point GetTilePos(Point pos) => new(pos.X * TileSize, pos.Y * TileSize);

    public int GetLeft() => 0;
    public int GetRight() => _tilemapSize.Width * TileSize;
    public int GetTop() => 0;
    public int GetBottom() => _tilemapSize.Height * TileSize;

    public Rectangle GetTileRect(Point pos) => GetTileRect(pos.X, pos.Y);

    public Rectangle GetTileRect(int x, int y) => new(x * TileSize, y * TileSize, TileSize, TileSize);
    
    public const int TileSize = 16;

    public static Tilemap LoadFromFile(string path)
    {
        var reader = File.OpenText(path);
        var width = int.Parse(reader.ReadLine() ?? "0");
        var height = int.Parse(reader.ReadLine() ?? "0");
        var startX = int.Parse(reader.ReadLine() ?? "0");
        var startY = int.Parse(reader.ReadLine() ?? "0");
        var endX = int.Parse(reader.ReadLine() ?? "0");
        var endY = int.Parse(reader.ReadLine() ?? "0");
        var size = new Size(width, height);
        var tiles = new int[height, width];
        var darkTiles = new int[height, width];
        var lightTiles = new int[height, width];
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width);
            
            for (int col = 0; col < size.Width; col++)
            {
                tiles[row, col] = line[col];
            }
        }
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width);
            
            for (int col = 0; col < size.Width; col++)
            {
                darkTiles[row, col] = line[col];
            }
        }
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width);
            
            for (int col = 0; col < size.Width; col++)
            {
                lightTiles[row, col] = line[col];
            }
        }

        reader.Dispose();

        return new Tilemap
        {
            _tiles = tiles,
            _darkTiles = darkTiles,
            _lightTiles = lightTiles,
            _tilemapSize = size,
            _spawnPoint = new(startX, startY),
            _endPoint = new(endX, endY),
        };
    }
    
    public void SaveToFile(string path)
    {
        var writer = File.CreateText(path);
        writer.WriteLine(_tilemapSize.Width);
        writer.WriteLine(_tilemapSize.Height);
        writer.WriteLine(_spawnPoint.X);
        writer.WriteLine(_spawnPoint.Y);
        writer.WriteLine(_endPoint.X);
        writer.WriteLine(_endPoint.Y);
        for (int row = 0; row < _tilemapSize.Height; row++)
        {
            for (int col = 0; col < _tilemapSize.Width; col++)
            {
                writer.Write((char)_tiles[row, col]);
            }
            writer.WriteLine();
        }
        for (int row = 0; row < _tilemapSize.Height; row++)
        {
            for (int col = 0; col < _tilemapSize.Width; col++)
            {
                writer.Write((char)_darkTiles[row, col]);
            }
            writer.WriteLine();
        }
        for (int row = 0; row < _tilemapSize.Height; row++)
        {
            for (int col = 0; col < _tilemapSize.Width; col++)
            {
                writer.Write((char)_lightTiles[row, col]);
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
        RenderLayer(spriteBatch, WorldMode.None, Color.White);
        RenderLayer(spriteBatch, UpfallCommon.CurrentWorldMode, Color.White);
    }

    public void EditorRender(SpriteBatch spriteBatch)
    {
        foreach (WorldMode mode in Enum.GetValues<WorldMode>())
        {
            Color color = Color.White;
            if (mode != UpfallCommon.CurrentWorldMode)
                color *= 0.33f;
            RenderLayer(spriteBatch, mode, color);
        }
    }

    private void RenderLayer(SpriteBatch spriteBatch, WorldMode mode, Color color)
    {
        int[,] layer = mode switch
        {
            WorldMode.Dark => _darkTiles,
            WorldMode.Light => _lightTiles,
            _ => _tiles,
        };

        for (int row = 0; row < _tiles.GetLength(0); row++)
        {
            for (int col = 0; col < _tiles.GetLength(1); col++)
            {
                int tile = layer[row, col];
                if (tile != 0)
                    spriteBatch.Draw(Assets.Pixel, new Vector2(col * TileSize, row * TileSize), null, color, 0f, Vector2.Zero, new Vector2(TileSize), SpriteEffects.None, 0f);
            }
        }
    }
}
