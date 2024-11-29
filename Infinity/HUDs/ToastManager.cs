using R3;

namespace fms.HUD;

public class ToastManager
{
    private readonly Subject<string> _focusEntered = new();

    public static ToastManager Instance { get; private set; }

    /// <summary>
    /// </summary>
    public Observable<string> FocusEntered => _focusEntered;

    static ToastManager()
    {
        Instance = new ToastManager();
    }

    public void CommitFocusEntered(string key)
    {
        _focusEntered.OnNext(key);
    }
}