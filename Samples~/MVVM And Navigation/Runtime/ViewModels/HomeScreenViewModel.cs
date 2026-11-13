using Unity.AppUI.MVVM;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    /// <summary>
    /// ViewModel for the Home screen.
    /// <c>IItemService</c> is resolved via constructor injection by the DI container.
    /// </summary>
    [ObservableObject]
    public partial class HomeScreenViewModel
    {
        readonly IItemService m_ItemService;

        [ObservableProperty]
        Item[] m_Items;

        public HomeScreenViewModel(IItemService itemService)
        {
            m_ItemService = itemService;
            Items = m_ItemService.GetItems();
        }

        [ICommand]
        void Refresh()
        {
            Items = m_ItemService.GetItems();
        }
    }
}
