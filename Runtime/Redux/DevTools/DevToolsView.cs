using System;
using Unity.AppUI.Redux.DevTools.States;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Redux.DevTools.UI
{
    /// <summary>
    /// The User Interface for the Redux DevTools.<br/>
    /// You can instantiate this class and add it to a visual tree to display the DevTools anywhere in your UI.
    /// </summary>
    class DevToolsView : VisualElement
    {
        Store m_Store;

        Dropdown m_StoreDropdown;

        /// <summary>
        /// Create a new instance of the DevTools GUI.
        /// </summary>
        public DevToolsView()
        {
            InitializeComponent();
        }

        void InitializeComponent()
        {
            var panel = new Panel
            {
                name = "root",
                scale = "small"
            };
            panel.AddToClassList("devtools-root");
            Add(panel);

            var toolbar = new Toolbar {dockMode = ToolbarDockMode.Top};
            m_StoreDropdown = new Dropdown();
            toolbar.Add(m_StoreDropdown);
            toolbar.Add(new Divider { direction = Direction.Horizontal });
            var actionGroup = new ActionGroup {direction = Direction.Horizontal};
            toolbar.Add(actionGroup);
            panel.Add(toolbar);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            CreateStore();
            m_Store.Dispatch(Actions.fetchStores.Invoke());
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            ReleaseStore();
        }

        void CreateStore()
        {
            m_Store = Store.CreateStore(new[]
            {
                Store.CreateSlice(
                    name: Actions.sliceName,
                    initialState: new AppState(),
                    reducers =>
                    {
                        reducers.AddCase(Actions.selectStore, Reducers.SetSelectedStore);
                        reducers.AddCase(Actions.startRecording, Reducers.SetIsRecording);
                    },
                    extraReducers =>
                    {
                        extraReducers.AddCase(Actions.fetchStores.fulfilled, Reducers.SetFoundStores);
                    }),
            });

            this.ProvideContext(new Contexts.StoreContext(m_Store));
        }

        void ReleaseStore()
        {
            m_Store = null;
        }
    }
}
