using System;
using Unity.AppUI.Core;
using Unity.AppUI.Redux.DevTools.States;

namespace Unity.AppUI.Redux.DevTools
{
    static partial class Actions
    {
        internal const string sliceName = "devTools";

        internal static readonly ActionCreator<FoundStore> selectStore = A(nameof(selectStore));

        internal static readonly AsyncThunkCreator<FoundStore[]> fetchStores = new(A(nameof(fetchStores)), Thunks.FetchStores);

        internal static readonly ActionCreator<bool> startRecording = A(nameof(startRecording));

        static string A(string type) => MemoryUtils.Concatenate(sliceName, "/", type);
    }
}
