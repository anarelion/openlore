using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.godot_resources.Intermediates;

[GlobalClass]
public partial class LoreTexture : Resource, ILoreResource<LoreTexture>
{
    private enum Mode
    {
        Static,
        Animated,
    }

    [Export] private Mode _textureType;
    [Export] private ImageTexture _texture;
    [Export] private Texture2DArray _textureArray;
    [Export] private Array<string> _textureArrayPaths = [];
    [Export] private int _animationDelay;
    [Export] private int _animationFrameCount;

    public LoreTexture()
    {
    }

    public LoreTexture(Frag04SimpleSpriteDef sprite, EqResourceLoader loader)
    {
        ResourceName = sprite.Name;
        var bitmapNames = sprite.GetAllBitmapNames();
        Array<Image> images = [];
        foreach (var image in bitmapNames.Select(loader.Get<LoreImage>))
        {
            images.Add(image);
            _textureArrayPaths.Add(image.ResourcePath);
        }

        if (sprite.Animated)
        {
            _textureType = Mode.Animated;
            _textureArray = new Texture2DArray();
            _textureArray.CreateFromImages(ExpandTextureArray(images));
            _animationDelay = sprite.AnimationDelayMs;
            _animationFrameCount = bitmapNames.Length;
            return;
        }

        _textureType = Mode.Static;
        _texture = ImageTexture.CreateFromImage(images[0]);
    }

    public static implicit operator ImageTexture(LoreTexture texture)
    {
        Trace.Assert(texture._textureType == Mode.Static);
        return texture._texture;
    }

    public static implicit operator Texture2DArray(LoreTexture texture)
    {
        Trace.Assert(texture._textureType == Mode.Animated);
        return texture._textureArray;
    }

    public int Count
    {
        get
        {
            Trace.Assert(_textureType == Mode.Animated);
            return _animationFrameCount;
        }
    }

    private static Array<Image> ExpandTextureArray(Array<Image> list)
    {
        var maxWidth = 0;
        var maxHeight = 0;
        foreach (var image in list)
        {
            if (image.GetHeight() > maxHeight) maxHeight = image.GetHeight();
            if (image.GetWidth() > maxWidth) maxWidth = image.GetWidth();
        }

        foreach (var image in list)
        {
            var originalWidth = image.GetWidth();
            if (image.GetWidth() < maxWidth)
            {
                image.Crop(maxWidth, image.GetHeight());
                for (var i = 1;
                     i < (maxWidth / originalWidth) + 1;
                     i++)
                {
                    image.BlitRect(image, new Rect2I(0, 0, originalWidth, image.GetHeight()),
                        new Vector2I(i * originalWidth, 0));
                }
            }

            var originalHeight = image.GetHeight();
            if (image.GetHeight() >= maxHeight) continue;
            {
                image.Crop(image.GetWidth(), maxHeight);
                for (var i = 1;
                     i < (maxHeight / originalHeight) + 1;
                     i++)
                {
                    image.BlitRect(image, new Rect2I(0, 0, image.GetWidth(), originalHeight),
                        new Vector2I(0, i * originalHeight));
                }
            }
        }

        return list;
    }


    public void Save(string path)
    {
        GD.Print($"LoreTexture: saving path: {path}");
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        var info = LoreFileHandlingUtils.StartDump(this);
        info.Add("images", _textureArrayPaths);
        info.Add("mode", (int)_textureType);
        if (_textureType == Mode.Animated)
        {
            info.Add("animation_delay", _animationDelay);
            info.Add("animation_frame_count", _animationFrameCount);
        }

        file.StoreVar(info);
    }

    public static LoreTexture Load(string path)
    {
        GD.Print($"LoreTexture: loading path: {path}");
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

        var info = (Dictionary<string, Variant>)file.GetVar();
        var result = new LoreTexture
        {
            _textureType = (Mode)(int)info["mode"],
            _textureArrayPaths = (Array<string>)info["images"]
        };

        Array<Image> images = [];
        foreach (var image in result._textureArrayPaths)
        {
            images.Add(LoreImage.Load(image));
        }

        if (result._textureType == Mode.Animated)
        {
            result._textureArray = new Texture2DArray();
            result._textureArray.CreateFromImages(ExpandTextureArray(images));
            result._animationDelay = (int)info["animation_delay"];
            result._animationFrameCount = (int)info["animation_frame_count"];
        }
        else
        {
            result._texture = ImageTexture.CreateFromImage(images[0]);
        }

        LoreFileHandlingUtils.ApplyDump(result, info);
        return result;
    }

    public static string GetExtension()
    {
        return ".lore_texture";
    }
}