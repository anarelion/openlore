using Godot;
using Godot.Collections;

namespace OpenLore.resource_manager.pack_file;

[GlobalClass]
public partial class PfsArchive : Resource
{
    [Export] public string LoadedPath;
    [Export] public Array<PfsFile> Files = [];
}