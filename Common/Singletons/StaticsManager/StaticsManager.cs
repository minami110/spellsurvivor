using Godot;

namespace fms;

/// <summary>
///     統計を収集するためのクラス
///     ダメージ表示も担当している
/// </summary>
public partial class StaticsManager : CanvasLayer
{
    [Export]
    private PackedScene _damageNumberScene = null!;

    [Export]
    private Node? _customDamageNumberContainer;

    public enum DamageTakeOwner
    {
        Player,
        Enemy
    }

    private static StaticsManager? _instance;

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
        // Config で表示設定がされていない場合は表示しない
        if (!GameConfig.Singleton.ShowDamageNumbers.Value)
        {
            return;
        }

        var damageNumberHud = _damageNumberScene.Instantiate<DamageNumberHud>();
        damageNumberHud.Hide();
        damageNumberHud.Damage = damage;
        if (ownerType == DamageTakeOwner.Player)
        {
            damageNumberHud.Color = new Color(1f, 0.5f, 0.5f);
        }
        else
        {
            damageNumberHud.Color = new Color(1f, 1f, 1f);
        }

        if (_customDamageNumberContainer is not null)
        {
            _customDamageNumberContainer.AddChild(damageNumberHud);
        }
        else
        {
            AddChild(damageNumberHud);
        }

        damageNumberHud.GlobalPosition = globalPosition;
        damageNumberHud.Show();
    }
}