using System;
using Godot;
using R3;

namespace fms;

public partial class ShopOwnItem : VBoxContainer
{
    [ExportGroup("Internal Reference")]
    [Export]
    private TextureRect _icon = null!;

    [Export]
    private Label _name = null!;

    [Export]
    private Label _level = null!;

    [Export]
    private Button _sellButton = null!;

    [Export]
    private Control _toolTipControl = null!;

    public WeaponBase Weapon { get; set; } = null!;

    public override void _Ready()
    {
        _icon.Texture = Weapon.Config.Sprite;
        _name.Text = Weapon.Config.Name;

        // Subscribe level
        var d1 = Weapon.State.Level.ChangedCurrentValue
            .Subscribe(this, (x, t) => { t._level.Text = $"(Lv.{x})"; });

        // ToDo: とりあえず買値と同じに..
        _sellButton.Text = $"Sell ${Weapon.Config.Price}";
        var d2 = _sellButton.PressedAsObservable().Subscribe(_ =>
        {
            throw new NotImplementedException();
            // Main.Shop.SellWeaponCard(Weapon.OwnedEntity, WeaponCard);
        });

        Disposable.Combine(d1, d2).AddTo(this);
    }
}