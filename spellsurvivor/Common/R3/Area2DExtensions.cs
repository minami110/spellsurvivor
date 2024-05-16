#nullable enable
using System.Threading;
using Godot;
using R3;

namespace spellsurvivor;

public static class Area2DExtensions
{
    public static Observable<Area2D> AreaEnteredAsObservable(this Area2D area2D, CancellationToken cancellationToken = default)
    {
        return Observable.FromEvent<Area2D.AreaEnteredEventHandler, Area2D>(
            h => new Area2D.AreaEnteredEventHandler(h),
            h => area2D.AreaEntered += h,
            h => area2D.AreaEntered -= h,
            cancellationToken
        );
    }
}