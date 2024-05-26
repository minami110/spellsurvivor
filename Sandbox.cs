using System;
using fms;
using Godot;
using R3;

public partial class Sandbox : Node
{
    private readonly Subject<Unit> _s = new();

    public override async void _EnterTree()
    {
        new A().AddTo(this);
    }

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        _s.AddTo(this);
        GD.Print("ready");
        await this.WaitForSecondsAsync(1f);
        QueueFree();
    }

    public override void _Notification(int what)
    {
    }

    public class A : IDisposable
    {
        public void Dispose()
        {
            GD.Print("A disposed");
        }
    }
}