using System;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples
{
    public class TrackpadSample : MonoBehaviour
    {
        public float dotMoveFactor = 10f;

        public float dotScaleFactor = 2f;

        public float dotMinSize = 1f;

        public float dotMaxSize = 10f;

        public float cameraZoomSpeed = 2f;

        public float cameraMoveSpeed = 2f;

        public float cameraMinSize = 0.5f;

        public float cameraMaxSize = 5f;

        public UIDocument uiDocument;

        VisualElement m_Dot;

        Camera m_Camera;

        Vector2 m_Offset;

        void Start()
        {
            m_Camera = GetComponent<Camera>();
            var panel = uiDocument.rootVisualElement.Q<Panel>("panel");
            m_Dot = panel.Q<VisualElement>("trackpad-viz__dot");
            panel.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            panel.RegisterCallback<WheelEvent>(OnWheel);
            panel.RegisterCallback<PinchGestureEvent>(OnMagnify);
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var size = m_Dot.parent.contentRect.size;
            var position = new Vector3(size.x / 2f, size.y / 2f, 0f);
#if UNITY_6000_2_OR_NEWER
            m_Dot.style.translate = position;
#else
            m_Dot.transform.position = position;
#endif
        }

        void OnMagnify(PinchGestureEvent evt)
        {
#if UNITY_6000_2_OR_NEWER
            var currentScale = m_Dot.resolvedStyle.scale.value;
#else
            var currentScale = m_Dot.transform.scale;
#endif
            var scale = evt.gesture.deltaMagnification * dotScaleFactor + currentScale.x;
            scale = Mathf.Clamp(scale, dotMinSize, dotMaxSize);
            var finalScale = new Vector3(scale, scale, 1f);
#if UNITY_6000_2_OR_NEWER
            m_Dot.style.scale = finalScale;
#else
            m_Dot.transform.scale = finalScale;
#endif
        }

        void OnWheel(WheelEvent evt)
        {
            if (evt.modifiers != EventModifiers.None)
                return;

            var size = m_Dot.parent.contentRect.size;
#if UNITY_6000_2_OR_NEWER
            var currentPosition = m_Dot.resolvedStyle.translate;
#else
            var currentPosition = m_Dot.transform.position;
#endif
            var x = Mathf.Clamp(currentPosition.x + evt.delta.x * dotMoveFactor, 0f, size.x);
            var y = Mathf.Clamp(currentPosition.y + evt.delta.y * dotMoveFactor, 0f, size.y);
            var newPosition = new Vector3(x, y, 0f);
#if UNITY_6000_2_OR_NEWER
            m_Dot.style.translate = newPosition;
#else
            m_Dot.transform.position = newPosition;
#endif
            m_Offset.x += evt.delta.x;
            m_Offset.y += evt.delta.y;
        }

        void Update()
        {
            var magGesture = AppUIInput.pinchGesture;
            if (magGesture.state is GestureRecognizerState.Changed or GestureRecognizerState.Began)
            {
                m_Camera.orthographicSize =
                    Mathf.Clamp(m_Camera.orthographicSize - magGesture.deltaMagnification * cameraZoomSpeed, cameraMinSize, cameraMaxSize);
            }

            var tr = transform;
            var pos = tr.position;
            tr.position = new Vector3(
                pos.x - m_Offset.x * cameraMoveSpeed * m_Camera.orthographicSize,
                pos.y,
                pos.z + m_Offset.y * cameraMoveSpeed * m_Camera.orthographicSize);

            m_Offset = Vector2.zero;
        }
    }
}
