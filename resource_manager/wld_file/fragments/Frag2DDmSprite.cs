﻿using Godot;

namespace OpenLore.resource_manager.wld_file.fragments;

// Lantern Extractor class
[GlobalClass]
public partial class Frag2DDmSprite : WldFragment
{
    [Export] public int MeshReference;
    [Export] public Frag36DmSpriteDef2 NewMesh;
    [Export] public Frag2CDmSpriteDef OldMesh;

    public override void Initialize(int index, int type, int size, byte[] data, WldFile wld, LoreResourceLoader loader)
    {
        base.Initialize(index, type, size, data, wld, loader);
        Name = wld.GetName(Reader.ReadInt32());
        MeshReference = Reader.ReadInt32();
        var fragment = wld.GetFragment(MeshReference);

        NewMesh = fragment as Frag36DmSpriteDef2;
        if (NewMesh != null) return;

        OldMesh = fragment as Frag2CDmSpriteDef;
        if (OldMesh != null) return;

        GD.PrintErr($"No mesh reference found for fragment {Index} pointing to {fragment.Index}");
    }
}