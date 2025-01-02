using Godot;
using Godot.Collections;

namespace OpenLore.resource_manager.godot_resources.Intermediates;

public static class LoreFileHandlingUtils
{
    private const string RESOURCE_META = "resource_meta";
    private const string RESOURCE_NAME = "resource_name";
    private const string RESOURCE_PATH = "resource_path";

    public static Dictionary<string, Variant> StartDump(Resource resource)
    {
        Dictionary<string, Variant> info = [];
        info.Add(RESOURCE_NAME, resource.ResourceName);
        info.Add(RESOURCE_PATH, resource.ResourcePath);
        info.Add(RESOURCE_META, GetMeta(resource));
        return info;
    }

    public static void ApplyDump(Resource resource, Dictionary<string, Variant> info)
    {
        resource.ResourceName = (string)info[RESOURCE_NAME];
        resource.TakeOverPath((string)info[RESOURCE_PATH]);
        ApplyMeta(resource, (Dictionary<string, Variant>)info[RESOURCE_META]);
    }

    private static Dictionary<string, Variant> GetMeta(Resource resource)
    {
        var result = new Dictionary<string, Variant>();
        foreach (var key in resource.GetMetaList())
        {
            result.Add(key, resource.GetMeta(key));
        }

        return result;
    }

    private static void ApplyMeta(Resource resource, Dictionary<string, Variant> meta)
    {
        foreach (var key in meta.Keys)
        {
            resource.SetMeta(key, meta[key]);
        }
    }
}