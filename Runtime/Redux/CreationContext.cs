namespace Unity.AppUI.Redux
{
    public partial class Store<TStoreState>
    {
        /// <summary>
        /// The creation context of the store.
        /// </summary>
        public class CreationContext
        {
            /// <summary>
            /// The store instance.
            /// </summary>
            public Store<TStoreState> store { get; }

            /// <summary>
            /// The dispatcher of the store.
            /// </summary>
            public Dispatcher dispatcher
            {
                get => store.m_Dispatcher;
                set => store.m_Dispatcher = value;
            }

            /// <summary>
            /// The reducer of the store.
            /// </summary>
            public Reducer<TStoreState> reducer
            {
                get => store.m_Reducer;
                set => store.m_Reducer = value;
            }

            /// <summary>
            /// The state of the store.
            /// </summary>
            public TStoreState state
            {
                get => store.m_State;
                set => store.m_State = value;
            }

            /// <summary>
            /// Creates a new instance of the context.
            /// </summary>
            internal CreationContext(Store<TStoreState> store)
            {
                this.store = store;
            }
        }
    }
}
