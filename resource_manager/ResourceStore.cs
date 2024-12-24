using System.Net.Sockets;
using Godot;
using Godot.Collections;

namespace OpenLore.resource_manager;

public class ResourceStore<[MustBeVariant] T> where T : Resource
{
    private readonly Dictionary<string, Array<T>> _items = [];

    public void Add(string key, T resource)
    {
        if (!_items.ContainsKey(key))
        {
            _items.Add(key, []);
        }

        _items[key].Add(resource);
    }

    public T Get(string key)
    {
        return _items.TryGetValue(key, out var value) ? value[^1] : null;
    }
    
    public int Count => _items.Count;
}