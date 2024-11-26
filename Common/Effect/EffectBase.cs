using Godot;
using ApplicationException = System.ApplicationException;

namespace fms;

public partial class EffectBase : Node
{
    private uint _lifeTime;
    public required uint Duration { get; init; }
    private protected IAttributeDictionary Dictionary { get; private set; } = null!;

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            AddToGroup(GroupNames.Effect);
            var dictionary = GetParentOrNull<IAttributeDictionary>();
            Dictionary = dictionary ??
                         throw new ApplicationException(
                             $"{nameof(EffectBase)} は {nameof(IAttributeDictionary)} を実装しているノードの子でなければなりません");
        }
        else if (what == NotificationReady)
        {
            // Duration が設定されている場合は Process を有効化して自動で消えるようにする
            if (Duration <= 0u)
            {
                return;
            }

            _lifeTime = Duration;
            SetPhysicsProcess(true);
        }
        else if (what == NotificationPhysicsProcess)
        {
            if (_lifeTime == 0u)
            {
                SetPhysicsProcess(false);
                QueueFree();
            }
            else
            {
                _lifeTime--;
            }
        }
    }
}