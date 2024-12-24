using System.Diagnostics;
using System.Linq;
using Godot;
using Godot.Collections;
using OpenLore.resource_manager.wld_file.fragments;

namespace OpenLore.resource_manager.godot_resources;

[GlobalClass]
public partial class LoreTexture : Resource
{
    enum Mode
    {
        Static,
        Animated,
    }

    [Export] private Mode _textureType;
    [Export] private ImageTexture _texture;
    [Export] private Texture2DArray _textureArray;
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
        foreach (var image in bitmapNames.Select(loader.GetImage))
        {
            images.Add(image);
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
}