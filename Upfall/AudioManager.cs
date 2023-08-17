using System;
using Brocco;

namespace Upfall;

public static class AudioManager
{
    private static int _volume = 10;

    public static int Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0, 10);
    }

    public static void PlaySound(string name)
    {
        var sound = Assets.GetSound(name);
        sound?.Play(_volume / 10f, 0f, 0f);
    }

    public static void PlayWorldSound(string name)
    {
        PlaySound(name + "_" + (UpfallCommon.CurrentWorldMode == WorldMode.Light ? "light" : "dark"));  // Light world sounds have a low-pass filter
    }
}
