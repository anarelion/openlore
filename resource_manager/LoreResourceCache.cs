using System;
using System.Linq;
using Godot.Collections;
using Godot;
using OpenLore.resource_manager.godot_resources.Intermediates;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class LoreResourceCache : Resource
{
    private const string CACHE_ROOT = "user://assets_cache";
    private const string INDEX_EXTENSION = ".tres";
    public const string DATA_EXTENSION = ".tres";
    
    [Export] public string Filename;
    [Export] public Dictionary<string, bool> Processed = [];
    [Export] public Dictionary<string, string> Data = [];

    public static LoreResourceCache Initialise(string filename)
    {
        var indexFile = $"{CACHE_ROOT}/{filename}_index{INDEX_EXTENSION}";
        EnsurePathExists(CACHE_ROOT);

        if (FileAccess.FileExists(indexFile))
        {
            return ResourceLoader.Load<LoreResourceCache>(indexFile);
        }

        var cache = new LoreResourceCache()
        {
            Filename = filename,
        };
        cache.Save();
        return cache;
    }

    private void Save()
    {
        var indexFile = $"{CACHE_ROOT}/{Filename}_index{INDEX_EXTENSION}";
        ResourceSaver.Save(this, indexFile, ResourceSaver.SaverFlags.Compress | ResourceSaver.SaverFlags.ChangePath);
    }

    public bool IsFullyProcessed()
    {
        return Processed.Count != 0 && Processed.Values.All(value => value);
    }

    public T Get<T>(string name) where T : Resource, ILoreResource<T>
    {
        return Get<T>(name, () => throw new NullReferenceException($"LoreResource cache miss {name}"));
    }

    public T Get<T>(string name, Func<T> getDataFunc) where T : Resource, ILoreResource<T>
    {
        if (Data.TryGetValue(name, out var cachedPath))
        {
            return T.Load(cachedPath);
        }
        
        EnsurePathExists($"{CACHE_ROOT}/{Filename}_data");
        var path = $"{CACHE_ROOT}/{Filename}_data/{name}{T.GetExtension()}";
        
        var data = getDataFunc();
        data.ResourcePath = path;
        data.Save(path);
        
        Data[name] = path;
        Processed[name] = true;
        Save();
        return data;
    }

    private static void EnsurePathExists(string path)
    {
        if (DirAccess.Open(path) != null) return;
        var root = DirAccess.Open("user://");
        root.MakeDirRecursive(path);
    }
}