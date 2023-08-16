using Microsoft.Xna.Framework;

namespace Upfall;

public enum Direction
{
    Up,
    Down,
    Left,
    Right,
}

public static class DirectionExtensions
{
    public static float ToRotation(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => MathHelper.PiOver2,
            Direction.Down => -MathHelper.PiOver2,
            Direction.Left => MathHelper.Pi,
            Direction.Right => 0f,
            _ => 0f,
        };
    }
}
