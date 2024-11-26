using Godot;

namespace fms;

public interface IAttributeDictionary
{
    void SetAttribute(string key, Variant value);
    bool TryGetAttribute(string key, out Variant value);
}