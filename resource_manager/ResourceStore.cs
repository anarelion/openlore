using Godot;
using Godot.Collections;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace OpenLore.resource_manager;

public class ResourceStore<[MustBeVariant] T> where T : Resource
{
    private readonly Dictionary<string, Array<T>> _byName = [];
    private readonly Dictionary<int, T> _byIndex = [];

    public void Add(int index, string key, T resource)
    {
        if (!_byName.ContainsKey(key))
        {
            _byName.Add(key, []);
        }

        _byName[key].Add(resource);
        _byIndex[index] = resource;
    }
    
    public T Get(int key)
    {
        return CollectionExtensions.GetValueOrDefault(_byIndex, key);
    }

    public T Get(string key)
    {
        return _byName.TryGetValue(key, out var value) ? value[^1] : null;
    }
    
    public int Count => _byName.Count;
}