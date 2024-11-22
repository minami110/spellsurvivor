using Godot;

namespace fms;

public interface IPawn
{
    /// <summary>
    /// 指定された方向に移動する
    /// </summary>
    /// <param name="direction"></param>
    public void MoveForward(in Vector2 direction);

    /// <summary>
    /// Primary が押されたときの処理
    /// </summary>
    public void PrimaryPressed();

    /// <summary>
    /// Primary が離されたときの処理
    /// </summary>
    public void PrimaryReleased();
}