using System.Linq;

namespace Unity.AppUI.Samples.MVVMNavigation
{
    public class ItemService : IItemService
    {
        static readonly Item[] k_Items =
        {
            new Item { id = "1", title = "Getting Started", description = "Learn the basics of App UI and how to set up your first project." },
            new Item { id = "2", title = "Navigation", description = "Explore the navigation system: NavHost, NavController, and NavGraph." },
            new Item { id = "3", title = "MVVM Pattern", description = "Understand ObservableObject, data binding, and the command pattern." },
            new Item { id = "4", title = "Dependency Injection", description = "Register and resolve services using the built-in DI container." },
            new Item { id = "5", title = "Theming", description = "Customize the look and feel of your application with USS variables." },
        };

        public Item[] GetItems()
        {
            return k_Items;
        }

        public Item GetItem(string id)
        {
            return k_Items.FirstOrDefault(item => item.id == id);
        }
    }
}
