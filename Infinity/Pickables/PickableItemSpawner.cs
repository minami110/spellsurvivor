using System.IO;
using Godot;
using Godot.Collections;

namespace fms;

public partial class PickableItemSpawner : Node
{
    private static PickableItemSpawner? _instance;

    private readonly Dictionary<string, PackedScene> _itemDict = new();

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


    public static void SpawnItem(string id, Vector2 position, bool isCallDeffered = true,
        Dictionary<string, float>? settings = null)
    {
        if (_instance is null)
        {
            GD.PrintErr($"[{nameof(PickableItemSpawner)}] Failed to access the instance");
            return;
        }

        if (!_instance._itemDict.TryGetValue(id, out var packedScene))
        {
            GD.PrintErr($"[{nameof(PickableItemSpawner)}] Item not found: {id}");
            return;
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
                if (fileName.EndsWith(".tscn"))
                {
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