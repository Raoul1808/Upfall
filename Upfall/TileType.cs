using Brocco;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public enum TileType : byte
{
    None,
    Solid,
}

public static class TileTypeExtensions
{
    public static Texture2D GetTextureForType(this TileType type)
    {
        switch (type)
        {
            case TileType.Solid:
                return Assets.Pixel;  // Tiles are really just white pixels
        
            default:
                return null;  // We probably shouldn't be rendering this
        }
    }
}
