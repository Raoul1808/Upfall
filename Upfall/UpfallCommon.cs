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
        if (!InEditor) return;
        var mode = _currentWorldMode;
        mode += 1;
        if (mode > WorldMode.Light)
            mode = WorldMode.None;
        _currentWorldMode = mode;
    }
    
    public static bool InEditor = false;
}
