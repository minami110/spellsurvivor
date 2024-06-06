using Godot;

namespace fms;

public interface IPawn
{
    public void MoveForward(in Vector2 direction);
    public void PrimaryPressed();
    public void PrimaryReleased();
}