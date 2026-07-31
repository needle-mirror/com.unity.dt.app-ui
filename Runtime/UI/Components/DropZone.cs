using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.Core;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container that accepts drag and drop operations for content placement.
    /// </summary>
    /// <remarks>
    /// The DropZone component provides a designated area where users can drag and drop content. It's particularly
    /// useful for file uploads, content organization, and any interface that requires drag-and-drop functionality.
    ///
    /// The component visualizes different states during drag operations, providing clear feedback to users about
    /// whether the content being dragged can be dropped in the zone.
    ///
    /// Note: The drag and drop functionality is currently only available in the Unity Editor environment.
    /// </remarks>
    /// <example>
    /// <para>Basic DropZone Setup</para>
    ///
    /// <para>Creating a simple file drop zone.</para>
    /// <code lang="csharp"><![CDATA[
    /// var dropZone = new DropZone();
    /// dropZone.style.width = new StyleLength(200);
    /// dropZone.style.height = new StyleLength(200);
    ///
    /// dropZone.controller.acceptDrag = (objects) => true; // Accept all drops
    /// dropZone.controller.dropped += (objects) => {
    ///     Debug.Log($"Received {objects.Count()} items");
    /// };
    ///
    /// rootElement.Add(dropZone);
    /// ]]></code>
    /// <para>UXML Definition</para>
    ///
    /// <para>Defining a DropZone in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:DropZone name="file-drop-zone"
    ///                     style="width: 200px; height: 200px;"
    ///                     visible-indicator="true" />
    /// </UXML>
    /// ]]></code>
    /// <para>Custom Styling</para>
    ///
    /// <para>Applying custom styles to DropZone states.</para>
    /// <code lang="csharp">
    /// .appui-dropzone {
    ///     background-color: rgba(255, 255, 255, 0.1);
    ///     border-radius: 4px;
    /// }
    ///
    /// .appui-dropzone--accept-drag {
    ///     background-color: rgba(0, 255, 0, 0.1);
    ///     border-color: green;
    /// }
    ///
    /// .appui-dropzone--reject-drag {
    ///     background-color: rgba(255, 0, 0, 0.1);
    ///     border-color: red;
    /// }
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("drag-and-drop")]
    public partial class DropZone : BaseVisualElement
    {
        /// <summary>
        /// The controller used to manage the drag and drop operations.
        /// </summary>
        public DropZoneController controller { get; }

        /// <summary>
        /// The DropZone main styling class.
        /// </summary>
        public const string ussClassName = "appui-dropzone";

        /// <summary>
        /// The DropZone frame styling class.
        /// </summary>
        public const string frameUssClassName = ussClassName + "__frame";

        /// <summary>
        /// The DropZone background styling class.
        /// </summary>
        public const string backgroundUssClassName = ussClassName + "__background";

        /// <summary>
        /// The DropZone state styling class.
        /// </summary>
        [EnumName("GetDropZoneStateUssClassName", typeof(DragAndDropState))]
        public const string stateUssClassName = ussClassName + "--";

        /// <summary>
        /// The DropZone visible indicator styling class.
        /// </summary>
        public const string visibleIndicatorUssClassName = ussClassName + "--visible-indicator";

        readonly ExVisualElement m_DropZoneFrame;

        readonly VisualElement m_Background;

        Pressable m_Clickable;

        bool m_VisibleIndicator;

        DragAndDropState m_DropZoneState;

        IVisualElementScheduledItem m_FrameAnimation;

        /// <summary>
        /// The container used to display the content.
        /// </summary>
        public override VisualElement contentContainer => m_DropZoneFrame;

        /// <summary>
        /// The state of the DropZone.
        /// </summary>
        public DragAndDropState state
        {
            get => m_DropZoneState;
            set
            {
                if (m_DropZoneState == value)
                    return;
                RemoveFromClassList(GetDropZoneStateUssClassName(m_DropZoneState));
                m_DropZoneState = value;
                AddToClassList(GetDropZoneStateUssClassName(m_DropZoneState));
                RefreshVisibleIndicatorStyle();
            }
        }

        /// <summary>
        /// The visible indicator state of the DropZone.
        /// </summary>
        public bool visibleIndicator
        {
            get => m_VisibleIndicator;
            set
            {
                if (m_VisibleIndicator == value)
                    return;
                m_VisibleIndicator = value;
                RefreshVisibleIndicatorStyle();
            }
        }

        bool isIndicatorVisible => (state == DragAndDropState.Default && m_VisibleIndicator) || (state != DragAndDropState.Default);

        /// <summary>
        /// Create a new DropZone.
        /// </summary>
        public DropZone()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            m_Background = new VisualElement { name = backgroundUssClassName, pickingMode = PickingMode.Ignore };
            m_Background.AddToClassList(backgroundUssClassName);
            hierarchy.Add(m_Background);

            m_DropZoneFrame = new ExVisualElement
            {
                name = frameUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders,
            };
            m_DropZoneFrame.AddToClassList(frameUssClassName);
            hierarchy.Add(m_DropZoneFrame);

            controller = new DropZoneController(OnControllerStateChanged);
            this.AddManipulator(controller);

            state = DragAndDropState.Default;
            visibleIndicator = false;
            generateVisualContent = OnGenerateVisualContent;
        }

        void OnControllerStateChanged(DragAndDropState controllerState) => state = controllerState;

        void OnGenerateVisualContent(MeshGenerationContext _) => RefreshAnimation();

        void RefreshVisibleIndicatorStyle()
        {
            EnableInClassList(visibleIndicatorUssClassName, isIndicatorVisible);
            RefreshAnimation();
        }

        void RefreshAnimation()
        {
            if (this.IsInvisible() || !isIndicatorVisible)
            {
                pickingMode = PickingMode.Ignore;
                m_FrameAnimation?.Pause();
                m_FrameAnimation = null;
                return;
            }

            pickingMode = PickingMode.Position;
            m_FrameAnimation ??= m_DropZoneFrame.schedule
                .Execute(m_DropZoneFrame.MarkDirtyRepaint)
                .Every(Styles.animationRefreshDelayMs);
        }

    }
}
