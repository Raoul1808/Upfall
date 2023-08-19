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
    private Tile[,] _tiles;
    private Tile[,] _darkTiles;
    private Tile[,] _lightTiles;
    private Size _tilemapSize;
    private Point _spawnPoint;
    private Point _endPoint;

    private int _keyCount;
    
    public string LevelName { get; set; }
    public string LevelAuthor { get; set; }
    public IPalette LevelPalette { get; set; }

    public Vector2 HalfTile => new(TileSize / 2f);

    private Tilemap()
    {
    }
    
    public Tilemap(Size size)
    {
        _tilemapSize = size;
        _tiles = new Tile[size.Height, size.Width];
        _darkTiles = new Tile[size.Height, size.Width];
        _lightTiles = new Tile[size.Height, size.Width];
        _keyCount = 0;
        LevelPalette = PaletteSystem.GetDefaultPalette();
    }

    public void SetCommonTile(Point pos, TileType tile, Direction direction)
    {
        _tiles[pos.Y, pos.X].TileId = tile;
        _tiles[pos.Y, pos.X].Direction = direction;
    }

    public void SetDarkTile(Point pos, TileType tile, Direction direction)
    {
        _darkTiles[pos.Y, pos.X].TileId = tile;
        _darkTiles[pos.Y, pos.X].Direction = direction;
    }

    public void SetLightTile(Point pos, TileType tile, Direction direction)
    {
        _lightTiles[pos.Y, pos.X].TileId = tile;
        _lightTiles[pos.Y, pos.X].Direction = direction;
    }

    public void SetSpawn(Point pos)
    {
        _spawnPoint = pos;
    }

    public void SetExit(Point pos)
    {
        _endPoint = pos;
    }

    public Tile GetTileCommon(int x, int y) => _tiles[y, x];

    public Tile GetTileDark(int x, int y) => _darkTiles[y, x];

    public Tile GetTileLight(int x, int y) => _lightTiles[y, x];

    public Tile GetTile(int x, int y)
    {
        Tile tile = GetTileCommon(x, y);
        if (tile.TileId == 0)
        {
            switch (UpfallCommon.CurrentWorldMode)
            {
                case WorldMode.Dark:
                    return GetTileDark(x, y);

                case WorldMode.Light:
                    return GetTileLight(x, y);
            }
        }

        return tile;
    }

    public void RemoveKey(int x, int y)
    {
        if (_keyCount <= 0)
            return;  // There are no keys left in the level. Return
        WorldMode mode;
        Tile tile = GetTileCommon(x, y);
        if (tile.TileId != TileType.Key)
        {
            // Key not found in common tiles, search further
            switch (UpfallCommon.CurrentWorldMode)
            {
                case WorldMode.Dark:
                    tile = GetTileDark(x, y);
                    break;

                case WorldMode.Light:
                    tile = GetTileLight(x, y);
                    break;
            }

            mode = UpfallCommon.CurrentWorldMode;

            if (tile.TileId != TileType.Key)
            {
                // Key not found. Break
                return;
            }
        }
        else
            mode = WorldMode.Common;

        _keyCount--;
        AudioManager.PlayWorldSound("pickup");

        if (_keyCount <= 0)
            AudioManager.PlayWorldSound("unlock");
        
        switch (mode)
        {
            case WorldMode.Common:
                SetCommonTile(new(x, y), TileType.None, Direction.Left);
                break;

            case WorldMode.Dark:
                SetDarkTile(new(x, y), TileType.None, Direction.Left);
                break;

            case WorldMode.Light:
                SetLightTile(new(x, y), TileType.None, Direction.Left);
                break;
        }
    }

    public Vector2 GetSpawnPos() => GetTilePos(_spawnPoint).ToVector2() + HalfTile;

    public Point GetTilePos(Point pos) => new(pos.X * TileSize, pos.Y * TileSize);

    public int GetLeft() => 0;
    public int GetRight() => _tilemapSize.Width * TileSize;
    public int GetTop() => 0;
    public int GetBottom() => _tilemapSize.Height * TileSize;

    public Rectangle GetTileRect(Point pos, Tile tile) => GetTileRect(pos.X, pos.Y, tile);

    public Rectangle GetTileRect(int x, int y, Tile tile) => GetTileRect(x, y, tile.TileId, tile.Direction);

    public Rectangle GetTileRect(int x, int y, TileType tileId, Direction direction)
    {
        const int spikeGap = 10;
        
        return tileId switch
        {
            TileType.Spike when direction == Direction.Up => new (x * TileSize, y * TileSize + spikeGap, TileSize, TileSize - spikeGap),
            TileType.Spike when direction == Direction.Down => new (x * TileSize, y * TileSize, TileSize, TileSize - spikeGap),
            TileType.Spike when direction == Direction.Left => new (x * TileSize + spikeGap, y * TileSize, TileSize - spikeGap, TileSize),
            TileType.Spike when direction == Direction.Right => new (x * TileSize, y * TileSize, TileSize - spikeGap, TileSize),
            TileType.Key => new (x * TileSize + 5, y * TileSize + 2, TileSize - 6, TileSize - 2),
            _ => new(x * TileSize, y * TileSize, TileSize, TileSize),
        };
    }

    public Rectangle GetExitRect(int x, int y)
    {
        const int doorGap = 1;
        return new(x * TileSize + doorGap, y * TileSize + doorGap, TileSize - doorGap * 2, TileSize - doorGap);
    }
    
    public const int TileSize = 16;

    public static Tilemap LoadFromFile(string path)
    {
        var reader = File.OpenText(path);
        string levelName = reader.ReadLine();
        string levelAuthor = reader.ReadLine();
        var palette = LoadPalette(reader);
        int width = int.Parse(reader.ReadLine() ?? "0");
        int height = int.Parse(reader.ReadLine() ?? "0");
        int startX = int.Parse(reader.ReadLine() ?? "0");
        int startY = int.Parse(reader.ReadLine() ?? "0");
        int endX = int.Parse(reader.ReadLine() ?? "0");
        int endY = int.Parse(reader.ReadLine() ?? "0");
        var size = new Size(width, height);
        var tiles = new Tile[height, width];
        var darkTiles = new Tile[height, width];
        var lightTiles = new Tile[height, width];
        int keyCount = 0;
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width * 2);
            
            for (int col = 0; col < size.Width * 2; col += 2)
            {
                var tileId = (TileType)line[col];
                if (tileId == TileType.Key)
                    keyCount++;
                tiles[row, col / 2].TileId = tileId;
                tiles[row, col / 2].Direction = (Direction)line[col + 1];
            }
        }
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width * 2);
            
            for (int col = 0; col < size.Width * 2; col += 2)
            {
                var tileId = (TileType)line[col];
                if (tileId == TileType.Key)
                    keyCount++;
                darkTiles[row, col / 2].TileId = (TileType)line[col];
                darkTiles[row, col / 2].Direction = (Direction)line[col + 1];
            }
        }
        for (int row = 0; row < size.Height; row++)
        {
            var line = reader.ReadLine() ?? new string('0', size.Width * 2);
            
            for (int col = 0; col < size.Width * 2; col += 2)
            {
                var tileId = (TileType)line[col];
                if (tileId == TileType.Key)
                    keyCount++;
                lightTiles[row, col / 2].TileId = (TileType)line[col];
                lightTiles[row, col / 2].Direction = (Direction)line[col + 1];
            }
        }

        reader.Dispose();

        if (palette is LerpPalette lerp)
        {
            Console.WriteLine("Lerp palette colors: " + lerp.DarkColor1 + " " + lerp.DarkColor2 + " " + lerp.LightColor1 + " " + lerp.LightColor2);
        }

        return new Tilemap
        {
            _tiles = tiles,
            _darkTiles = darkTiles,
            _lightTiles = lightTiles,
            _tilemapSize = size,
            _spawnPoint = new(startX, startY),
            _endPoint = new(endX, endY),
            _keyCount = keyCount,
            LevelName = levelName,
            LevelAuthor = levelAuthor,
            LevelPalette = palette,
        };
    }

    private static IPalette LoadPalette(StreamReader reader)
    {
        string line = reader.ReadLine();
        PaletteType palette = (PaletteType)(line?[0] ?? -1);
        switch (palette)
        {
            case PaletteType.Simple:
                string dark = reader.ReadLine();
                string light = reader.ReadLine();
                return new SimplePalette
                {
                    DarkColor = ColorUtil.HexToCol(dark),
                    LightColor = ColorUtil.HexToCol(light),
                };
            case PaletteType.Lerp:
                string dark1 = reader.ReadLine();
                string dark2 = reader.ReadLine();
                string light1 = reader.ReadLine();
                string light2 = reader.ReadLine();
                return new LerpPalette
                {
                    DarkColor1 = ColorUtil.HexToCol(dark1),
                    DarkColor2 = ColorUtil.HexToCol(dark2),
                    LightColor1 = ColorUtil.HexToCol(light1),
                    LightColor2 = ColorUtil.HexToCol(light2),
                };
            case PaletteType.Trippy:
                return new TrippyPalette();
            
            default:
                return PaletteSystem.GetDefaultPalette();
        }
    }

    private void SavePalette(StreamWriter writer)
    {
        writer.WriteLine((char)LevelPalette.PaletteType);
        if (LevelPalette is SimplePalette simple)
        {
            writer.WriteLine(ColorUtil.ColToHex(simple.DarkColor));
            writer.WriteLine(ColorUtil.ColToHex(simple.LightColor));
            return;
        }

        if (LevelPalette is LerpPalette lerp)
        {
            writer.WriteLine(ColorUtil.ColToHex(lerp.DarkColor1));
            writer.WriteLine(ColorUtil.ColToHex(lerp.DarkColor2));
            writer.WriteLine(ColorUtil.ColToHex(lerp.LightColor1));
            writer.WriteLine(ColorUtil.ColToHex(lerp.LightColor2));
            return;
        }

        if (LevelPalette is TrippyPalette)
        {
            // Nothing configurable on the trippy palette
            return;
        }
    }
    
    public void SaveToFile(string path)
    {
        var writer = File.CreateText(path);
        writer.WriteLine(LevelName);
        writer.WriteLine(LevelAuthor);
        SavePalette(writer);
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
                var tile = _tiles[row, col];
                writer.Write((char)tile.TileId);
                writer.Write((char)tile.Direction);
            }
            writer.WriteLine();
        }
        for (int row = 0; row < _tilemapSize.Height; row++)
        {
            for (int col = 0; col < _tilemapSize.Width; col++)
            {
                var tile = _darkTiles[row, col];
                writer.Write((char)tile.TileId);
                writer.Write((char)tile.Direction);
            }
            writer.WriteLine();
        }
        for (int row = 0; row < _tilemapSize.Height; row++)
        {
            for (int col = 0; col < _tilemapSize.Width; col++)
            {
                var tile = _lightTiles[row, col];
                writer.Write((char)tile.TileId);
                writer.Write((char)tile.Direction);
            }
            writer.WriteLine();
        }
        writer.Flush();
        writer.Dispose();
    }

    private string RectToStr(Rectangle rect) => $"{{Left: {rect.Left}, Right: {rect.Right}, Top: {rect.Top}, Bottom: {rect.Bottom}}}";
    
    public void SolveCollisions(Player entity)
    {
        if (_keyCount <= 0 && entity.BoundingBox.Intersects(GetExitRect(_endPoint.X, _endPoint.Y)))
        {
            // Trigger level win immediately
            entity.Win();
            return;  // We don't want to process collisions
        }
        var pos = entity.Position / TileSize;
        int lx = Math.Max((int)pos.X - 1, 0);
        int ux = Math.Min((int)pos.X + 2, _tiles.GetLength(1) - 1);
        int ly = Math.Max((int)pos.Y - 1, 0);
        int uy = Math.Min((int)pos.Y + 2, _tiles.GetLength(0) - 1);

        var tileRects = new List<Tuple<float, Tile, Rectangle>>();

        bool kill = false;

        for (int x = lx; x <= ux; x++)
        {
            for (int y = ly; y <= uy; y++)
            {
                var bbox = entity.BoundingBox;
                Tile tile = GetTile(x, y);
                var tileRect = GetTileRect(x, y, tile);
                if (tile.TileId != 0 && bbox.Intersects(tileRect))
                {
                    if (tile.TileId == TileType.Key)
                    {
                        RemoveKey(x, y);
                        continue;  // Collect the key and restart collision checking
                    }
                    
                    if (tile.TileId.IsLethal())
                    {
                        kill = true;
                    }
                    tileRects.Add(new(BroccoMath.Distance(bbox.Center, tileRect.Center), tile, tileRect));
                }
            }
        }

        if (kill)
        {
            entity.Kill();
            return;
        }

        if (tileRects.Count <= 0) return;  // No collision detected?
        
        tileRects.Sort((x, y) => x.Item1.CompareTo(y.Item1));

        bool shouldCrossPortal = true;  // We don't want the player to cross the portal multiple times per frame
        
        foreach (var (_, tile, tileRect) in tileRects)
        {
            // TODO: the collision code REALLY shouldn't be here, but I REALLY NEED to make progress in the game.
            if (!entity.BoundingBox.Intersects(tileRect)) continue;

            if (tile.TileId == TileType.Solid)
            {
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

            if (shouldCrossPortal && tile.TileId == TileType.Portal)
            {
                // This basically boils down to this
                // 1. Get the player's previous position from his current velocity
                // 2. Compare the player's previous position and the current position to the portal's center position.
                // 3. If the player was before the portal in one frame and beyond the portal on the next frame, the player effectively crossed the portal.
                // 4. If the portal was crossed, do not attempt to cross another portal.
                
                Point portalCenter = tileRect.Center;
                bool crossedPortal = false;
                float playerPos = 0f;
                float playerPreVel = 0f;
                switch (tile.Direction)
                {
                    case Direction.Down:
                        playerPos = entity.Position.Y;
                        playerPreVel = playerPos - entity.Velocity.Y;
                        if (playerPreVel < portalCenter.Y && playerPos >= portalCenter.Y)
                            crossedPortal = true;
                        break;

                    case Direction.Up:
                        playerPos = entity.Position.Y;
                        playerPreVel = playerPos - entity.Velocity.Y;
                        if (playerPreVel > portalCenter.Y && playerPos <= portalCenter.Y)
                            crossedPortal = true;
                        break;

                    case Direction.Left:
                        playerPos = entity.Position.X;
                        playerPreVel = playerPos - entity.Velocity.X;
                        if (playerPreVel > portalCenter.X && playerPos <= portalCenter.X)
                            crossedPortal = true;
                        break;

                    case Direction.Right:
                        playerPos = entity.Position.X;
                        playerPreVel = playerPos - entity.Velocity.X;
                        if (playerPreVel < portalCenter.X && playerPos >= portalCenter.X)
                            crossedPortal = true;
                        break;
                }
                if (crossedPortal)
                {
                    UpfallCommon.CycleWorldMode();
                    shouldCrossPortal = false;
                }
            }
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        RenderLayer(spriteBatch, WorldMode.Common, Color.Fuchsia);
        RenderLayer(spriteBatch, WorldMode.Dark, Color.Red);
        RenderLayer(spriteBatch, WorldMode.Light, Color.Blue);
        RenderTile(spriteBatch, Color.White, _endPoint, TileType.ExitDoor);
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

        RenderTile(spriteBatch, Color.White, _spawnPoint, TileType.Spawn);
        RenderTile(spriteBatch, Color.White, _endPoint, TileType.ExitDoor);
    }

    private void RenderLayer(SpriteBatch spriteBatch, WorldMode mode, Color color)
    {
        Tile[,] layer = mode switch
        {
            WorldMode.Dark => _darkTiles,
            WorldMode.Light => _lightTiles,
            _ => _tiles,
        };

        for (int row = 0; row < _tiles.GetLength(0); row++)
        {
            for (int col = 0; col < _tiles.GetLength(1); col++)
            {
                Tile tile = layer[row, col];
                RenderTile(spriteBatch, color, new(col, row), tile);
            }
        }
    }

    public void RenderTile(SpriteBatch spriteBatch, Color color, Point pos, Tile tile)
    {
        RenderTile(spriteBatch, color, pos, tile.TileId, tile.Direction);
    }

    public void RenderTile(SpriteBatch spriteBatch, Color color, Point pos, TileType tileId)
    {
        RenderTile(spriteBatch, color, pos, tileId, Direction.Right);
    }

    public void RenderTile(SpriteBatch spriteBatch, Color color, Point pos, TileType tileId, Direction direction)
    {
        if (tileId != TileType.None)
        {
            if (!UpfallCommon.InEditor && tileId == TileType.ExitDoor && _keyCount > 0)
                tileId = TileType.LockedDoor;
            var tex = tileId.GetTextureForType() ?? Assets.Pixel;
            float rot = tileId.HasDirection() ? direction.ToRotation() : 0f;
            var rect = GetTileRect(pos.X, pos.Y, TileType.Solid, direction);
            rect.X += (int)HalfTile.X;
            rect.Y += (int)HalfTile.Y;
            var offset = HalfTile;

            Rectangle? srcRect = tileId == TileType.Portal ? AnimationHelper.GetPortalSourceRectangle() : null;
            spriteBatch.Draw(tex, rect, srcRect, color, -rot, offset, SpriteEffects.None, 0f);
        }
    }
}
