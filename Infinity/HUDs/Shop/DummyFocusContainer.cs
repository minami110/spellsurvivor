using Godot;
using R3;

namespace fms.HUD;

/// <summary>
/// 背景に配置するダミーのボタン, 表示中の Toast を非表示にするためダミーのフォーカスを発行する
/// </summary>
public partial class DummyFocusContainer : Control
{
    [Export]
    private string _focusKey = "Shop/Dummy";

    public override void _Ready()
    {
        this.FocusEnteredAsObservable()
            .Subscribe(this, (_, self) => { ToastManager.Instance.CommitFocusEntered(self._focusKey); })
            .AddTo(this);
    }
}