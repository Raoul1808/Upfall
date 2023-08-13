using System;
using System.IO;
using Brocco;
using Brocco.Basic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Upfall.Entities;

namespace Upfall;

public class Tilemap
{
    private int[,] _tiles;

    public Tilemap(int[,] tiles)
    {
        _tiles = tiles;
    }

    public Tilemap(Size size)
    {
        _tiles = new int[size.Height, size.Width];
    }

    public void SetTile(Point pos, int tile)
    {
        _tiles[pos.Y, pos.X] = tile;
    }
    
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
    
    public void SolveCollisions(Player entity)
    {
        var pos = entity.Position / TileSize;
        int lx = Math.Max((int)pos.X - 1, 0);
        int ux = Math.Min((int)pos.X + 2, _tiles.GetLength(1) - 1);
        int ly = Math.Max((int)pos.Y - 1, 0);
        int uy = Math.Min((int)pos.Y + 2, _tiles.GetLength(0) - 1);

        for (int x = lx; x <= ux; x++)
        {
            for (int y = ly; y <= uy; y++)
            {
                if (_tiles[y, x] != 0)
                    entity.ResolveTileCollision(new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
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
                    spriteBatch.Draw(Assets.Pixel, new Vector2(col * TileSize, row * TileSize), null, Color.Black, 0f, Vector2.Zero, new Vector2(TileSize), SpriteEffects.None, 0f);
            }
        }
    }
}
