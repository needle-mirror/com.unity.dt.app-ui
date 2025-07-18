using Unity.AppUI.Redux;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete

namespace Unity.AppUI.Samples.MVVMRedux
{
    public interface IStoreService
    {
        Store store { get; }
    }
}
