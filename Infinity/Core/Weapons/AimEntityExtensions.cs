using System;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using R3;

namespace fms;

public sealed class AimEntityEnterTargetWaiter(AimEntity aimEntity) : IDisposable
{
    private IDisposable? _waitEnterEntityDisposable;

    public void Cancel()
    {
        _waitEnterEntityDisposable?.Dispose();
        _waitEnterEntityDisposable = null;
    }

    public void Start(float angleDegree, Action callback)
    {
        if (aimEntity.IsAiming)
        {
            callback();
        }
        else
        {
            if (_waitEnterEntityDisposable is not null)
            {
                throw new InvalidProgramException("Already waiting for an entity to enter");
            }

            _waitEnterEntityDisposable = aimEntity.EnteredAnyEntity
                .SubscribeAwait(async (_, token) =>
                {
                    // 5度以内に狙いを定めるまで待機する
                    await WaitForAimTargetAsync(angleDegree, token);

                    if (!token.IsCancellationRequested && aimEntity.IsAiming)
                    {
                        _waitEnterEntityDisposable?.Dispose();
                        _waitEnterEntityDisposable = null;
                        callback();
                    }
                }, AwaitOperation.Drop);
        }
    }

    private async ValueTask WaitForAimTargetAsync(float angleDegree,
        CancellationToken token)
    {
        var angle = Mathf.DegToRad(angleDegree);
        while (true)
        {
            if (token.IsCancellationRequested || !aimEntity.IsAiming)
            {
                return;
            }

            await aimEntity.NextPhysicsFrameAsync();

            if (Mathf.Abs(aimEntity.AngleDiff) <= angle)
            {
                return;
            }
        }
    }

    public void Dispose()
    {
        Cancel();
    }
}