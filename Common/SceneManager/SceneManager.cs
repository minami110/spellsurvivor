using System;
using System.Threading.Tasks;
using Godot;
using Array = Godot.Collections.Array;

namespace fms;

public partial class SceneManager : Node
{
    public void GoToScene(string packedScenePath)
    {
        if (!ResourceLoader.Exists(packedScenePath))
        {
            throw new InvalidProgramException($"Specified scene does not exist: {packedScenePath}");
        }

        CallDeferred(MethodName.GoToSceneInternal, packedScenePath);
    }

    private async void GoToSceneInternal(string packedScenePath)
    {
        var tree = GetTree();

        // 現在のシーンを削除する
        var currentScene = tree.CurrentScene;
        if (!IsInstanceValid(currentScene))
        {
            GD.PushError("Failed to get valid current scene");
            return;
        }

        GD.Print($"[{nameof(Title)}] Free current scene: {currentScene.GetPath()}");
        currentScene.Free();

        // ToDo: ロード画面を用意する

        // 別スレッドでの PackedScene の読み込みを開始する
        const ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Ignore;

        GD.Print($"[{nameof(Title)}] Loading request started\n" +
                 $"  path: {packedScenePath}\n" +
                 $"  useSubThreads: {true}\n" +
                 $"  cache mode: {cacheMode}");

        var error = ResourceLoader.LoadThreadedRequest(
            packedScenePath,
            "PackedScene",
            true,
            cacheMode
        );

        if (error != Error.Ok)
        {
            GD.PushError($"Failed to load {packedScenePath}, error: {error}");
            return;
        }

        // ロードが終了するまで待機する
        var success = await WaitLoadThreadedRequestAsync(packedScenePath);
        if (!success)
        {
            GD.PushError($"Failed to load {packedScenePath}");
            return;
        }

        GD.Print($"[{nameof(Title)}] Completed loading!");

        // シーンを読み込む
        var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(packedScenePath);
        var scene = packedScene.Instantiate();

        // シーンを追加する
        tree.Root.AddChild(scene);
        tree.CurrentScene = scene;
        GD.Print($"[{nameof(Title)}] Updated current scene: {tree.CurrentScene.GetPath()}");
    }

    private async Task<bool> WaitLoadThreadedRequestAsync(string resourcePath)
    {
        var loadingProgress = new Array();
        while (true)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(resourcePath, loadingProgress);
            GD.Print($"  progress: {(float)loadingProgress[0] * 100f: 000.0} %");

            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.InProgress:
                    break;
                case ResourceLoader.ThreadLoadStatus.Loaded:
                    return true;
                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                case ResourceLoader.ThreadLoadStatus.Failed:
                default:
                    return false;
            }

            // ロード中
            await this.BeginOfProcessAsync();
        }
    }
}