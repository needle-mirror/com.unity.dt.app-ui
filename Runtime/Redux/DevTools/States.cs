using System;

namespace Unity.AppUI.Redux.DevTools.States
{
    [Serializable]
    record RecordedAction
    {
        public string type;
    }

    [Serializable]
    record FoundStore
    {
        public string name;
        public string id;

        public RecordedAction[] recordedActions;

        public override string ToString()
        {
            return name;
        }
    }

    [Serializable]
    record AppState
    {
        public FoundStore[] stores = null;

        public FoundStore selectedStore = null;

        public bool recording = false;
    }
}
