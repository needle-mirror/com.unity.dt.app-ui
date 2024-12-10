using System.Threading.Tasks;
using Unity.AppUI.Redux.DevTools.States;

namespace Unity.AppUI.Redux.DevTools
{
    static class Thunks
    {
        internal static async Task<FoundStore[]> FetchStores(IThunkAPI<FoundStore[]> api)
        {
            return await Task.FromResult(new FoundStore[] { });
        }
    }
}
