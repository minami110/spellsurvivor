using Godot;
using R3;

namespace fms;

[GlobalClass]
public partial class LabelHasToolTip : Label
{
    [Export(PropertyHint.MultilineText)]
    public string ToolTipText = string.Empty;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        var d1 = this.MouseEnteredAsObservable().Subscribe(this, (_, state) =>
        {
            ToolTipToast.Text = state.ToolTipText;
            ToolTipToast.Show();
        });
        var d2 = this.MouseExitedAsObservable().Subscribe(_ => ToolTipToast.Hide());
        Disposable.Combine(d1, d2).AddTo(this);
    }
}