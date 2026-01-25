using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples
{
    public class TrackpadSample : MonoBehaviour
    {
        public float moveFactor = 0.5f;

        public float scaleFactor = 2f;

        public UIDocument uiDocument;

        VisualElement m_Dot;

        Vector2 m_DotPosition = new Vector2(0.5f, 0.5f);

        float m_DotScale = 1f;

        Vector2 m_PendingPanDelta;

        float m_PendingMagnificationDelta;

        void Start()
        {
            var panel = uiDocument.rootVisualElement.Q<Panel>("panel");
            m_Dot = panel.Q<VisualElement>("trackpad-viz__dot");

            // Use WheelEvent for panning (trackpad scroll gesture)
            panel.RegisterCallback<WheelEvent>(OnWheel);

            // Use PinchGestureEvent for pinch/zoom gesture (dispatched by Panel)
            panel.RegisterCallback<PinchGestureEvent>(OnPinch);

            m_Dot.parent.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            m_Dot.parent.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            // Initialize dot position
            var size = m_Dot.parent.contentRect.size;
            var newPosition = new Vector3(m_DotPosition.x * size.x, m_DotPosition.y * size.y, 0f);
#if UNITY_6000_2_OR_NEWER
            m_Dot.style.translate = newPosition;
#else
            m_Dot.transform.position = newPosition;
#endif
        }

        void OnPinch(PinchGestureEvent evt)
        {
            // Store the magnification delta for 3D transform update
            m_PendingMagnificationDelta = evt.gesture.deltaMagnification;

            // Update the UI dot scale
            m_DotScale += evt.gesture.deltaMagnification;
            m_DotScale = Mathf.Clamp(m_DotScale, 0.1f, 10f);
            var newScale = new Vector3(m_DotScale, m_DotScale, 1f);
#if UNITY_6000_2_OR_NEWER
            m_Dot.style.scale = newScale;
#else
            m_Dot.transform.scale = newScale;
#endif
        }

        void OnWheel(WheelEvent evt)
        {
            // Check if this is a trackpad gesture (button == touchPadId) without Ctrl/Cmd modifier
            // When Ctrl/Cmd is pressed, it's a zoom gesture handled by OnPinch
            if (evt.ctrlKey || evt.commandKey)
                return;

            // Store the pan delta for 3D transform update
            var panDelta = evt.delta;
            m_PendingPanDelta = panDelta;

            // Update the UI dot position
            var size = m_Dot.parent.contentRect.size;
            if (size.x > 0 && size.y > 0)
            {
                m_DotPosition.x -= panDelta.x / size.x * 0.1f;
                m_DotPosition.y -= panDelta.y / size.y * 0.1f;
                m_DotPosition.x = Mathf.Clamp01(m_DotPosition.x);
                m_DotPosition.y = Mathf.Clamp01(m_DotPosition.y);

                var newPosition = new Vector3(m_DotPosition.x * size.x, m_DotPosition.y * size.y, 0f);
#if UNITY_6000_2_OR_NEWER
                m_Dot.style.translate = newPosition;
#else
                m_Dot.transform.position = newPosition;
#endif
            }
        }

        void Update()
        {
            var tr = transform;

            // Apply pending pan to 3D transform
            if (m_PendingPanDelta != Vector2.zero)
            {
                var pos = tr.position;
                tr.position = new Vector3(
                    pos.x - m_PendingPanDelta.x * moveFactor,
                    pos.y,
                    pos.z + m_PendingPanDelta.y * moveFactor);
                m_PendingPanDelta = Vector2.zero;
            }

            // Apply pending magnification to 3D transform
            if (!Mathf.Approximately(m_PendingMagnificationDelta, 0f))
            {
                var scale = tr.localScale.x + m_PendingMagnificationDelta * scaleFactor;
                scale = Mathf.Clamp(scale, 0.001f, 100f);
                tr.localScale = new Vector3(scale, scale, scale);
                m_PendingMagnificationDelta = 0f;
            }
        }
    }
}
