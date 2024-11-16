using Godot;

namespace fms;

public interface IPawn
{
    /// <summary>
    /// 現在狙っている方向を取得する
    /// </summary>
    /// <returns></returns>
    public Vector2 GetAimDirection();

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