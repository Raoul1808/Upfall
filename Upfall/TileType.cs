using System;
using Brocco;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public enum TileType : byte
{
    None,
    Solid,
    UpSpike,
    DownSpike,
    LeftSpike,
    RightSpike,
}

public static class TileTypeExtensions
{
    public static Texture2D GetTextureForType(this TileType type)
    {
        return type switch
        {
            TileType.Solid => Assets.Pixel,  // Tiles are really just white pixels
            TileType.UpSpike => Assets.GetTexture("spike"),
            TileType.DownSpike => Assets.GetTexture("spike"),
            TileType.LeftSpike => Assets.GetTexture("spike"),
            TileType.RightSpike => Assets.GetTexture("spike"),
            _ => null
        };
    }

    public static float GetRotationForType(this TileType type)
    {
        return type switch
        {
            TileType.UpSpike => 0f,
            TileType.DownSpike => MathHelper.Pi,
            TileType.LeftSpike => MathHelper.PiOver2,
            TileType.RightSpike => -MathHelper.PiOver2,
            _ => 0f,
        };
    }

    public static bool IsLethal(this TileType type)
    {
        return type switch
        {
            TileType.UpSpike => true,
            TileType.DownSpike => true,
            TileType.LeftSpike => true,
            TileType.RightSpike => true,
            _ => false,
        };
    }
}
