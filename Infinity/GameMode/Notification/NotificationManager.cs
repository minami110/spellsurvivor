using Godot;

namespace fms;

public partial class NotificationManager : Node
{
    [Export]
    private PackedScene _damageNumberScene = null!;

    [Export]
    private Node _damageNumberContainer = null!;

    public enum DamageTakeOwner
    {
        Player,
        Enemy
    }

    private static NotificationManager? _instance;

    public override void _EnterTree()
    {
        _instance = this;
    }

    public override void _ExitTree()
    {
        _instance = null;
    }

    public static void CommitDamage(DamageTakeOwner ownerType, float amount, in Vector2 globalPosition)
    {
        if (_instance is null)
        {
            return;
        }

        if (_instance.IsQueuedForDeletion() || !IsInstanceValid(_instance))
        {
            return;
        }

        _instance.SendDamageInternal(ownerType, amount, globalPosition);
    }

    private void SendDamageInternal(DamageTakeOwner ownerType, float damage, in Vector2 globalPosition)
    {
        if (GameConfig.Singleton.ShowDamageNumbers.Value)
        {
            var damageNumber = _damageNumberScene.Instantiate<DamageNumberHud>();
            damageNumber.Hide();
            damageNumber.Damage = damage;
            if (ownerType == DamageTakeOwner.Player)
            {
                damageNumber.Color = new Color(1f, 0.5f, 0.5f);
            }
            else
            {
                damageNumber.Color = new Color(1f, 1f, 1f);
            }

            _damageNumberContainer.AddChild(damageNumber);
            damageNumber.GlobalPosition = globalPosition;
            damageNumber.Show();
        }
    }
}