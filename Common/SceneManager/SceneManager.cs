using System;
using System.Threading.Tasks;
using Godot;
using Array = Godot.Collections.Array;

namespace fms;

public partial class SceneManager : Node
{
    private readonly Array _loadingProgress = new();
    private bool _isSceneChanging;

    public void GoToScene(string packedScenePath)
    {
        if (_isSceneChanging)
        {
            GD.PushError("Scene is already changing");
            return;
        }

        if (!ResourceLoader.Exists(packedScenePath))
        {
            throw new InvalidProgramException($"Specified scene does not exist: {packedScenePath}");
        }

        _isSceneChanging = true;
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
            _isSceneChanging = false;
            return;
        }

        this.DebugLog($"Free current scene: {currentScene.GetPath()}");
        currentScene.Free();

        // ToDo: ロード画面を用意する

        // 別スレッドでの PackedScene の読み込みを開始する
        const ResourceLoader.CacheMode cacheMode = ResourceLoader.CacheMode.Ignore;

        this.DebugLog("Loading request started\n" +
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
            _isSceneChanging = false;
            return;
        }

        // ロードが終了するまで待機する
        var success = await WaitLoadThreadedRequestAsync(packedScenePath);
        if (!success)
        {
            GD.PushError($"Failed to load {packedScenePath}");
            _isSceneChanging = false;
            return;
        }

        this.DebugLog("[Completed loading!");

        // シーンを読み込む
        var packedScene = (PackedScene)ResourceLoader.LoadThreadedGet(packedScenePath);
        var scene = packedScene.Instantiate();

        // シーンを追加する
        tree.Root.AddChild(scene);
        tree.CurrentScene = scene;
        GD.Print($"[{nameof(Title)}] Updated current scene: {tree.CurrentScene.GetPath()}");
        _isSceneChanging = false;
    }

    private async ValueTask<bool> WaitLoadThreadedRequestAsync(string resourcePath)
    {
        _loadingProgress.Clear();
        while (true)
        {
            var status = ResourceLoader.LoadThreadedGetStatus(resourcePath, _loadingProgress);
            GD.Print($"  progress: {(float)_loadingProgress[0] * 100f: 000.0} %");

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