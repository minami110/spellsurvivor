using System.Collections.Generic;
using System.IO;
using Godot;

namespace fms;

public partial class PickableItemSpawner : Node
{
    private static PickableItemSpawner? _instance;

    private readonly Godot.Collections.Dictionary<string, PackedScene> _itemDict = new();

    public override void _Notification(int what)
    {
        if (what == NotificationEnterTree)
        {
            _instance = this;
            GatherPickableItems();
        }
        else if (what == NotificationExitTree)
        {
            _instance = null;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="position"></param>
    /// <param name="isCallDeffered"></param>
    /// <param name="settings"></param>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
    public static void SpawnItem(string id, Vector2 position, bool isCallDeffered = true, Godot.Collections.Dictionary<string, float>? settings = null)
    {
        if (_instance is null)
        {
            GD.PrintErr($"[{nameof(PickableItemSpawner)}] Failed to access the instance");
            return;
        }

        if (!_instance._itemDict.TryGetValue(id, out var packedScene))
        {
            throw new KeyNotFoundException($"[{nameof(PickableItemSpawner)}] Item not found: {id}");
        }

        var pickableItem = packedScene.Instantiate<Node2D>();
        pickableItem.GlobalPosition = position;

        // ToDo: オプションを渡せるようにする

        if (isCallDeffered)
        {
            _instance.CallDeferred(Node.MethodName.AddChild, pickableItem);
        }
        else
        {
            _instance.AddChild(pickableItem);
        }

        _instance.DebugLog($"New item spawned: {id} at {position}");
    }

    private void GatherPickableItems()
    {
        const string _SEARCH_DIR = "res://Infinity/Pickables/";
        this.DebugLog($"Start loading pickable items from: {_SEARCH_DIR}");
        using var dir = DirAccess.Open(_SEARCH_DIR);
        if (dir != null)
        {
            dir.ListDirBegin();
            var fileName = dir.GetNext();
            while (fileName != string.Empty)
            {
                // Note: Godot 4.2.2
                // Runtime で XXX.tres.remap となっていることがある (ランダム?)
                // この場合 .remap を抜いたパスを読み込むとちゃんと行ける
                // See https://github.com/godotengine/godot/issues/66014
                if (fileName.EndsWith(".tscn") || fileName.EndsWith(".tscn.remap"))
                {
                    if (fileName.EndsWith(".remap"))
                    {
                        fileName = fileName.Replace(".remap", string.Empty);
                    }

                    var path = Path.Combine(_SEARCH_DIR, fileName);
                    var packedScene = GD.Load<PackedScene>(path);
                    // Remove extension
                    var n = fileName.Substring(0, fileName.LastIndexOf('.'));
                    GD.Print($"  Loaded: {n}.tscn");
                    _itemDict[n] = packedScene;
                }

                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }
        else
        {
            throw new DirectoryNotFoundException($"Directory not found: {_SEARCH_DIR}");
        }

        this.DebugLog("Completed!");
    }
}