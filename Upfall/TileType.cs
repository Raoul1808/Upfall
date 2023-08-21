using Brocco;
using Microsoft.Xna.Framework.Graphics;

namespace Upfall;

public enum TileType : byte
{
    None,
    Solid,
    Spike,
    Portal,
    Key,
    Spawn,  // Used only for editor
    LockedDoor,  // Used only for visuals
    ExitDoor,
}

public static class TileTypeExtensions
{
    public static Texture2D GetTextureForType(this TileType type)
    {
        return type switch
        {
            TileType.Solid => Assets.GetTexture("tile"),
            TileType.Spike => Assets.GetTexture("spike"),
            TileType.ExitDoor => Assets.GetTexture("door"),
            TileType.LockedDoor => Assets.GetTexture("door_locked"),
            TileType.Spawn => Assets.GetTexture("player"),
            TileType.Portal when UpfallCommon.InEditor => Assets.GetTexture("portal_editor"),
            TileType.Portal when !UpfallCommon.InEditor => Assets.GetTexture("portal_anim"),
            TileType.Key => Assets.GetTexture("key"),
            _ => null
        };
    }

    public static bool HasDirection(this TileType type)
    {
        return type switch
        {
            TileType.None => false,
            TileType.Solid => false,
            TileType.Spike => true,
            TileType.Portal => true,
            TileType.Spawn => false,
            TileType.ExitDoor => false,
            TileType.Key => false,
            _ => false,
        };
    }

    public static bool IsLethal(this TileType type)
    {
        return type switch
        {
            TileType.Spike => true,
            _ => false,
        };
    }
}
