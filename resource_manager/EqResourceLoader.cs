using System.IO;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenLore.GameController;
using OpenLore.resource_manager.file_formats.parsers;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.godot_resources.Intermediates;
using OpenLore.resource_manager.pack_file;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class EqResourceLoader : Node
{
    [Export] public string Filename;
    [Export] public string RequestedFileName;
    [Export] public bool Loaded;
    [Export] public bool Failed;
    [Export] public int AgeCounter;
    [Export] private Godot.Collections.Dictionary<int, string> _fragmentCacheKey = [];
    private LoreResourceCache _cache;

    public override void _Ready()
    {
        RequestedFileName = RequestedFileName.ToLower();
        GD.Print($"EqResourceLoader: requesting {RequestedFileName} at age {AgeCounter}");
        var assetPath = GameConfig.Instance.AssetPath;
        Filename = TestFiles([
            $"{assetPath}/{RequestedFileName}",
            $"{assetPath}/{RequestedFileName}.eqg",
            $"{assetPath}/{RequestedFileName}.s3d"
        ]);
        if (Filename == null)
        {
            GD.PrintErr($"EqResourceLoader: {RequestedFileName} doesn't exist!");
            Loaded = true;
            Failed = true;
            return;
        }

        _cache = LoreResourceCache.Initialise(RequestedFileName);
        if (_cache.IsFullyProcessed())
        {
            Loaded = true;
            return;
        }

        var archive = PfsParser.Load(Filename);
        Array<PfsFile> others = [];

        foreach (var file in archive.Files)
        {
            if ((file.FileBytes[0] == 'D' && file.FileBytes[1] == 'D' && file.FileBytes[2] == 'S')
                || (file.FileBytes[0] == 'B' && file.FileBytes[1] == 'M'))
            {
                var file1 = file;
                _cache.Get(file.Name, () => new LoreImage(file1));
                continue;
            }

            others.Add(file);
        }

        foreach (var file in others)
        {
            var wld = WldParser.Parse(file);
            if (wld == null) continue;

            foreach (var frag04 in wld.GetFragmentsOfType<Frag04SimpleSpriteDef>())
            {
                var textureCacheKey = $"{frag04.Name}_{frag04.Index}";
                _cache.Get(textureCacheKey, () => new LoreTexture(frag04, this));
                _fragmentCacheKey.Add(frag04.Index, textureCacheKey);
            }
        }
    }

    public override void _Process(double delta)
    {
    }

    public T Get<T>(int index) where T : Resource, ILoreResource<T>
    {
        return _cache.Get<T>(_fragmentCacheKey[index]);
    }

    public T Get<T>(string name) where T : Resource, ILoreResource<T>
    {
        return _cache.Get<T>(name);
    }

    public LoreImage GetImage(string imageName)
    {
        // return Failed ? null : Images.Get(imageName);
        return null;
    }

    public Resource GetActor(string tag)
    {
        // return Failed ? null : ActorDefs.GetValueOrDefault(tag);
        return null;
    }

    public Godot.Collections.Dictionary<string, ActorSkeletonPath> GetAnimationsFor(string tag)
    {
        Godot.Collections.Dictionary<string, ActorSkeletonPath> result = [];
        // if (Failed) return result;
        // foreach (var animation in ExtraAnimations.Values)
        // {
        //     if (animation.ActorName != tag) continue;
        //     result[(animation.AnimationName, animation.BoneName)] = animation;
        // }
        //
        return result;
    }

    private static string TestFiles(string[] names)
    {
        return names.FirstOrDefault(File.Exists);
    }

    // private async Task<bool> ProcessS3DFile(PfsArchive archive)
    // {
    //     GD.Print($"EqResourceLoader: processing S3D {Filename} - images {Images.Count}");
    //     foreach (var file in archive.Files)
    //     {
    //         if (file.Name != "objects.wld") continue;
    //         var wld = WldParser.Parse(file);
    //         GD.PrintErr($"EqResourceLoader: {Name} contains objects.wld but is unsupported");
    //         break;
    //     }
    //
    //     foreach (var file in archive.Files)
    //     {
    //         if (file.Name != "lights.wld") continue;
    //         var wld = WldParser.Parse(file);
    //         GD.PrintErr($"EqResourceLoader: {Name} contains lights.wld but is unsupported");
    //         break;
    //     }
    //
    //     foreach (var file in archive.Files)
    //     {
    //         if (file.Name != $"{Name}.wld") continue;
    //         var wld = WldParser.Parse(file);
    //         foreach (var frag04 in wld.GetFragmentsOfType<Frag04SimpleSpriteDef>())
    //         {
    //             Textures.Add(frag04.Index, frag04.Name, new LoreTexture(frag04, this));
    //         }
    //
    //         foreach (var frag30 in wld.GetFragmentsOfType<Frag30MaterialDef>())
    //         {
    //             Materials.Add(frag30.Index, frag30.Name, new LoreMaterial(frag30, this));
    //         }
    //
    //         break;
    //     }
    //
    //     return true;
    // }
    //
    // private async Task<bool> ProcessEQGFile(PfsArchive archive)
    // {
    //     GD.Print($"EqResourceLoader: processing EQG {Filename} - images {Images.Count}");
    //     return true;
    // }
}