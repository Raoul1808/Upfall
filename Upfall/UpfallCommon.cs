using SDL2;

namespace Upfall;

public static class UpfallCommon
{
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
            mode = InEditor ? WorldMode.None : WorldMode.Dark;
        CurrentWorldMode = mode;
    }
    
    public static bool InEditor = false;
    public static bool Playtesting = false;
    private static float _dt = 0f;

    public static float DeltaTime
    {
        get
        {
            var dt = _dt;
            _dt += 0.001f;
            return dt;
        }
    }
}
