using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using OpenLore.GameController;
using OpenLore.resource_manager.file_formats.parsers;
using OpenLore.resource_manager.godot_resources;
using OpenLore.resource_manager.pack_file;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager;

[GlobalClass]
public partial class EqResourceLoader : Node
{
    [Export] public string FileName;
    [Export] public string RequestedFileName;
    [Export] public bool Loaded;
    [Export] public bool Failed;
    [Export] public int AgeCounter;

    private readonly ResourceStore<LoreImage> _images = new();
    private readonly ResourceStore<LoreTexture> _textures = new();
    [Export] public Godot.Collections.Dictionary<int, Material> Materials = [];
    [Export] public Godot.Collections.Dictionary<int, ArrayMesh> Meshes = [];
    [Export] public Godot.Collections.Dictionary<string, Resource> ActorDefs = [];
    [Export] public Godot.Collections.Dictionary<int, ActorSkeletonPath> ExtraAnimations = [];

    private Task<bool> _task;

    public override void _Ready()
    {
        _task = Task.Run(async () => await LoadFile(RequestedFileName));
    }

    public override void _Process(double delta)
    {
        if (Loaded || _task == null || !_task.IsCompleted) return;

        if (_task.IsFaulted || _task.Result == false)
        {
            Failed = true;
        }

        Loaded = true;
        _task = null;

        GD.Print($"EqResourceLoader: completed processing {Name} age {AgeCounter} failed {Failed}");
    }

    public LoreImage GetImage(string imageName)
    {
        return Failed ? null : _images.Get(imageName);
    }

    public Resource GetActor(string tag)
    {
        return Failed ? null : ActorDefs.GetValueOrDefault(tag);
    }

    public Dictionary<(string, string), ActorSkeletonPath> GetAnimationsFor(string tag)
    {
        Dictionary<(string, string), ActorSkeletonPath> result = [];
        if (Failed) return result;
        foreach (var animation in ExtraAnimations.Values)
        {
            if (animation.ActorName != tag) continue;
            result[(animation.AnimationName, animation.BoneName)] = animation;
        }

        return result;
    }

    private async Task<bool> LoadFile(string name)
    {
        GD.Print($"EqResourceLoader: requesting {name} at age {AgeCounter}");
        var assetPath = GameConfig.Instance.AssetPath;
        FileName = await TestFiles([$"{assetPath}/{name}", $"{assetPath}/{name}.eqg", $"{assetPath}/{name}.s3d"]);
        if (FileName == null)
        {
            GD.PrintErr($"EqResourceLoader: {name} doesn't exist!");
            return false;
        }

        var archive = await PfsParser.Load(FileName);

        List<Task<(string, LoreImage)>> tasks = [];
        for (var i = 0; i < archive.Files.Count; i++)
        {
            if (archive.Files[i] is not PfsFile file)
            {
                GD.PrintErr($"File is not PFSFile on index {i}");
                continue;
            }

            if ((file.FileBytes[0] == 'D' && file.FileBytes[1] == 'D' && file.FileBytes[2] == 'S')
                || (file.FileBytes[0] == 'B' && file.FileBytes[1] == 'M'))
            {
                var task = Task.Run(() => (file.Name, new LoreImage(file)));
                tasks.Add(task);
            }
        }

        var imageResults = await Task.WhenAll([..tasks]);
        foreach (var im in imageResults)
        {
            _images.Add(im.Item1, im.Item2);
        }

        if (FileName.EndsWith(".s3d"))
        {
            return await ProcessS3DFile(archive);
        }

        if (FileName.EndsWith(".eqg"))
        {
            return await ProcessEQGFile(archive);
        }

        GD.PrintErr($"EqResourceLoader: {name} is an eqg and unsupported!");
        return false;
    }

    private static async Task<string> TestFiles(string[] names)
    {
        List<Task<string>> tasks = [];
        tasks.AddRange(names.Select(name => Task.Run(() => File.Exists(name) ? name : null)));

        var results = await Task.WhenAll([..tasks]);
        return results.FirstOrDefault(result => result != null);
    }

    private async Task<bool> ProcessS3DFile(PfsArchive archive)
    {
        GD.Print($"EqResourceLoader: processing S3D {FileName} - images {_images.Count}");
        foreach (var file in archive.Files)
        {
            if (file.Name != "objects.wld") continue;
            var wld = WldParser.Parse(file);
            GD.PrintErr($"EqResourceLoader: {Name} contains objects.wld but is unsupported");
            break;
        }

        foreach (var file in archive.Files)
        {
            if (file.Name != "lights.wld") continue;
            var wld = WldParser.Parse(file);
            GD.PrintErr($"EqResourceLoader: {Name} contains lights.wld but is unsupported");
            break;
        }

        foreach (var file in archive.Files)
        {
            if (file.Name != $"{Name}.wld") continue;
            var wld = WldParser.Parse(file);
            foreach (var frag04 in wld.GetFragmentsOfType<Frag04SimpleSpriteDef>())
            {
                _textures.Add(frag04.Name, new LoreTexture(frag04, this));
            }

            break;
        }

        return true;
    }

    private async Task<bool> ProcessEQGFile(PfsArchive archive)
    {
        GD.Print($"EqResourceLoader: processing EQG {FileName} - images {_images.Count}");
        return true;
    }
}