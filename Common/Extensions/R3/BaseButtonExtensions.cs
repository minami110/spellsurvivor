using System.Threading;
using Godot;

namespace R3;

public static class BaseButtonExtensions
{
    public static Observable<Unit> ButtonDownAsObservable(this BaseButton node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.ButtonDown += h,
            h => node.ButtonDown -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> ButtonUpAsObservable(this BaseButton node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.ButtonUp += h,
            h => node.ButtonUp -= h,
            cancellationToken
        );
    }

    public static Observable<Unit> PressedAsObservable(this BaseButton node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent(
            h => node.Pressed += h,
            h => node.Pressed -= h,
            cancellationToken
        );
    }

    public static Observable<bool> ToggledAsObservable(this BaseButton node,
        CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<BaseButton.ToggledEventHandler, bool>(
            h => new BaseButton.ToggledEventHandler(h),
            h => node.Toggled += h,
            h => node.Toggled -= h,
            cancellationToken
        );
    }
}