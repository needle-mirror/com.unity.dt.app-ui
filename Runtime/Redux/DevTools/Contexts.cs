using Unity.AppUI.Core;

namespace Unity.AppUI.Redux.DevTools.Contexts
{
    record StoreContext(Store store) : IContext
    {
        public Store store { get; } = store;
    }
}
