namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// Provides access to <see cref="Item"/> data.
    /// Registered as a singleton in the DI container so all ViewModels share the same instance.
    /// </summary>
    public interface IItemService
    {
        Item[] GetItems();
        Item GetItem(string id);
    }
}
