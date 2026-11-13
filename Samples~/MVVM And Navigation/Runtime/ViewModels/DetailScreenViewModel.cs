using Unity.AppUI.MVVM;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// ViewModel for the Detail screen.
    /// <c>IItemService</c> is resolved via constructor injection by the DI container.
    /// The item to display is loaded via <see cref="LoadItem"/> after navigation arguments are available.
    /// </summary>
    [ObservableObject]
    public partial class DetailScreenViewModel
    {
        readonly IItemService m_ItemService;

        [ObservableProperty]
        string m_Title;

        [ObservableProperty]
        string m_Description;

        public DetailScreenViewModel(IItemService itemService)
        {
            m_ItemService = itemService;
        }

        public void LoadItem(string itemId)
        {
            var item = m_ItemService.GetItem(itemId);
            if (item != null)
            {
                Title = item.title;
                Description = item.description;
            }
            else
            {
                Title = "Not Found";
                Description = $"No item with id '{itemId}' exists.";
            }
        }
    }
}
