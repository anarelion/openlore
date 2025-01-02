namespace OpenLore.resource_manager.godot_resources.Intermediates;

public interface ILoreResource<out T>
{
    public static abstract T Load(string path);

    public void Save(string path);

    public static abstract string GetExtension();
}