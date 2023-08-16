using Microsoft.Xna.Framework;

namespace Upfall;

// TODO: make an actual animation class
public static class AnimationHelper
{
    private const int PortalFrames = 16;
    private const int PortalFrameTime = 6;
    private static int _currentTimer = 0;
    private static int _currentFrame = 0;

    public static void UpdateFrames()
    {
        if (_currentTimer++ >= PortalFrameTime)
        {
            _currentTimer = 0;
            if (_currentFrame++ >= PortalFrames - 1)
            {
                _currentFrame = 0;
            }
        }
    }
    
    public static Rectangle GetPortalSourceRectangle()
    {
        if (UpfallCommon.InEditor)
            return new Rectangle(0, 0, 16, 16);
        return new Rectangle(16 * _currentFrame, 0, 16, 16);
    }
}
