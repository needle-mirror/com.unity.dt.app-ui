using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A SplitView is a container that displays two children at a time and provides a UI to
    /// navigate between them. The split bar can be dragged to resize the two children.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class SplitView : TwoPaneSplitView
    {
        /// <summary>
        /// The main styling class of the SplitView. This is the class that is used in the USS file.
        /// </summary>
        public const string ussClassName = "appui-splitview";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SplitView()
        {
            AddToClassList(ussClassName);
        }

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Defines the UxmlFactory for the SplitView.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="SplitView"/>.
        /// </summary>
        public new class UxmlTraits : TwoPaneSplitView.UxmlTraits
        {

        }
        
#endif
    }
}
