using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace Upfall;

public static class UpfallCommon
{
    #if DEBUG
    public const string Version = "1.0.0-DEBUG";
    public const string ShortVersion = "1.0-DEBUG";
    #else
    public const string Version = "1.0.0";
    public const string ShortVersion = "1.0";
    #endif
    
    private static WorldMode _currentWorldMode;

    public delegate void WorldChangeEvent(WorldMode oldMode, WorldMode newMode);

    public static event WorldChangeEvent OnWorldChange;
    
    public static WorldMode CurrentWorldMode
    {
        get => _currentWorldMode;
        set
        {
            var oldValue = _currentWorldMode;
            _currentWorldMode = value;
            OnWorldChange?.Invoke(oldValue, value);
        }
    }

    public static void CycleWorldMode()
    {
        var mode = _currentWorldMode;
        mode += 1;
        if (mode > WorldMode.Light)
            mode = InEditor ? WorldMode.Common : WorldMode.Dark;
        CurrentWorldMode = mode;
    }
    
    public static bool InEditor = false;
    public static bool Playtesting = false;

    public static readonly string GamePath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName ?? "";

    public static readonly string TilesPath = Path.Join(GamePath, "Content", "Maps");

    public static Vector2 ScreenCenter;
    public static Vector2 CanvasCenter;

    // Levels and shit
    
    private static List<string> _currentLevelSet;
    private static int _currentLevel;
    private static string _previousLevelTextDisplayed = string.Empty;

    public static void SetLevelSet(List<string> levelset, int startingLevel = 0)
    {
        _currentLevelSet = levelset;
        _currentLevel = startingLevel;
    }

    public static bool HasNextLevel()
    {
        return _currentLevel < _currentLevelSet.Count - 1;
    }

    public static void NextLevel() => _currentLevel++;

    public static Tilemap LoadCurrentLevel() => LoadLevel(_currentLevel);

    public static Tilemap LoadLevel(int level)
    {
        level = Math.Clamp(level, 0, _currentLevelSet.Count - 1);
        string levelPath = _currentLevelSet[level];
        var tilemap = Tilemap.LoadFromFile(levelPath);
        if (_previousLevelTextDisplayed != levelPath)
        {
            _previousLevelTextDisplayed = levelPath;
            NotificationSystem.ShowLevelName(tilemap.LevelName, tilemap.LevelAuthor);
        }
        return tilemap;
    }

    public static void ResetPreviousLevelTextDisplayed() => _previousLevelTextDisplayed = string.Empty;
}
