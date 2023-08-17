using System;
using Brocco;
using Microsoft.Xna.Framework;

namespace Upfall;

public interface IPalette
{
    PaletteType PaletteType { get; }
    Color DarkColor { get; }
    Color LightColor { get; }
    void UpdateColors(float dt);
}

public enum PaletteType
{
    Simple,
    Lerp,
    Trippy,
}

public readonly struct SimplePalette : IPalette
{
    public PaletteType PaletteType => PaletteType.Simple;
    
    public Color DarkColor { get; init; }
    public Color LightColor { get; init; }
    
    public void UpdateColors(float dt)
    {
    }

    public bool Equals(SimplePalette other)
    {
        return DarkColor.Equals(other.DarkColor) && LightColor.Equals(other.LightColor);
    }

    public override bool Equals(object obj)
    {
        return obj is SimplePalette other && Equals(other);
    }

    public static bool operator ==(SimplePalette left, SimplePalette right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SimplePalette left, SimplePalette right)
    {
        return !(left == right);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(DarkColor, LightColor);
    }
}

public struct LerpPalette : IPalette
{
    public PaletteType PaletteType => PaletteType.Lerp;

    private Color _lerpColor1;
    private Color _lerpColor2;
    
    private float _lerpTimer;
    private bool _oneToTwo;
    
    public Color DarkColor1 { get; init; }
    public Color DarkColor2 { get; init; }
    public Color LightColor1 { get; init; }
    public Color LightColor2 { get; init; }

    public Color DarkColor => _lerpColor1;
    public Color LightColor => _lerpColor2;

    public LerpPalette()
    {
        _oneToTwo = true;
        _lerpTimer = 0f;
    }
    
    public void UpdateColors(float dt)
    {
        const float lerpTime = 3f;
        
        if (_oneToTwo)
            _lerpTimer += dt / lerpTime;
        else
            _lerpTimer -= dt / lerpTime;

        if (_lerpTimer <= 0f)
        {
            _lerpTimer = 0f;
            _oneToTwo = true;
        }

        if (_lerpTimer >= 1f)
        {
            _lerpTimer = 1f;
            _oneToTwo = false;
        }

        _lerpColor1 = Color.Lerp(DarkColor1, DarkColor2, (float)Easings.InOutQuad(_lerpTimer));
        _lerpColor2 = Color.Lerp(LightColor1, LightColor2, (float)Easings.InOutQuad(_lerpTimer));
    }

    public bool Equals(LerpPalette other)
    {
        return DarkColor1.Equals(other.DarkColor1) &&
               DarkColor2.Equals(other.DarkColor2) &&
               LightColor1.Equals(other.LightColor1) &&
               LightColor2.Equals(other.LightColor2);
    }

    public override bool Equals(object obj)
    {
        return obj is LerpPalette other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(DarkColor1, DarkColor2, LightColor1, LightColor2);
    }
}

public struct TrippyPalette : IPalette
{
    public PaletteType PaletteType => PaletteType.Trippy;

    private Color _color1;
    private Color _color2;

    public Color DarkColor => _color1;
    public Color LightColor => _color2;

    public TrippyPalette()
    {
        _color1 = Color.Blue;
        _color2 = Color.Yellow;
    }
    
    public void UpdateColors(float dt)
    {
        ColorUtil.IncreaseHueBy(ref _color1, 1, out _);
        ColorUtil.IncreaseHueBy(ref _color2, 1, out _);
    }

    public override bool Equals(object obj)
    {
        return obj is TrippyPalette;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(DarkColor, LightColor);
    }
}

public class PaletteSystem : BroccoAutoSystem
{
    private static Color _lastDarkColor;
    private static Color _lastLightColor;
    
    private static IPalette _currentPalette;
    private static float _paletteTimer = 0f;

    public override void PreInitialize(BroccoGame game)
    {
        _currentPalette = GetDefaultPalette();
        _paletteTimer = 0f;
        _lastDarkColor = Color.Black;
        _lastLightColor = Color.White;
    }

    public override void PostUpdate(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _currentPalette.UpdateColors(dt);
        Color currentDark, currentLight;
        if (_paletteTimer > 0f)
        {
            _paletteTimer -= dt;
            var lerp = 1 - _paletteTimer;
            currentDark = Color.Lerp(_lastDarkColor, _currentPalette.DarkColor, (float)Easings.OutQuart(lerp));
            currentLight = Color.Lerp(_lastLightColor, _currentPalette.LightColor, (float)Easings.OutQuart(lerp));
        }
        else
        {
            currentDark = _currentPalette.DarkColor;
            currentLight = _currentPalette.LightColor;
        }
        ShaderEffectSystem.SetDarkColor(currentDark);
        ShaderEffectSystem.SetLightColor(currentLight);
    }

    public static void SetPalette(IPalette palette, float time = 1f)
    {
        if (_currentPalette.Equals(palette)) return;  // We don't want to re-set the palette
        _paletteTimer = time;
        _lastDarkColor = _currentPalette.DarkColor;
        _lastLightColor = _currentPalette.LightColor;
        _currentPalette = palette;
    }

    public static void ResetPalette(float time = 1f)
    {
        SetPalette(GetDefaultPalette(), time);
    }

    public static IPalette GetDefaultPalette() => new SimplePalette { DarkColor = Color.Black, LightColor = Color.White };

    public static IPalette GetDefaultLerpPalette() => new LerpPalette { DarkColor1 = Color.Black, DarkColor2 = Color.Black, LightColor1 = Color.White, LightColor2 = Color.White };
}
