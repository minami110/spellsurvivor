using System.Diagnostics;
using fms;
using Godot;
using Array = Godot.Collections.Array;

public partial class Sandbox : Node
{
    [Export(PropertyHint.File, "*.tscn")]
    private string _scenePath = string.Empty;

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        var error = ResourceLoader.LoadThreadedRequest(_scenePath);
        GD.Print($"error: {error}");

        if (error != Error.Ok)
        {
            GD.PushError($"Failed to load {_scenePath}, error: {error}");
            return;
        }

        var sw = Stopwatch.StartNew();
        var progress = new Array();
        while (true)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(_scenePath, progress);
            GD.Print($"[{sw}] status: {status} / progress: {progress}");

            if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            {
                GD.Print($"[{sw}] Completed");
                sw.Stop();

                // Get loaded resouce
                var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(_scenePath);

                // Add Tree
                var scene = packedScene.Instantiate();
                AddChild(scene);

                break;
            }

            await this.BeginOfProcessAsync();
        }
    }
}