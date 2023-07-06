using System;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Unity.AppUI.Samples
{
    public class TrackpadSample : MonoBehaviour
    {
        public float moveFactor = 20f;

        public float scaleFactor = 2f;

        public UIDocument uiDocument;

        VisualElement m_Dot;

        void Start()
        {
            var panel = uiDocument.rootVisualElement.Q<Panel>("panel");
            m_Dot = panel.Q<VisualElement>("trackpad-viz__dot");
            panel.RegisterCallback<PanGestureEvent>(OnPan);
            panel.RegisterCallback<MagnificationGestureEvent>(OnMagnify);
        }

        void OnMagnify(MagnificationGestureEvent evt)
        {
            var scale = evt.gesture.deltaMagnification + m_Dot.transform.scale.x;
            m_Dot.transform.scale = new Vector3(scale, scale, 1f);
        }

        void OnPan(PanGestureEvent evt)
        {
            var size = m_Dot.parent.contentRect.size;
            m_Dot.transform.position = new Vector3(evt.gesture.position.x * size.x, (1f - evt.gesture.position.y) * size.y, 0f);
        }

        void Update()
        {
            var tr = transform;
            var pos = tr.position;

            var panGesture = Platform.panGesture;
            if (panGesture.phase == TouchPhase.Moved)
                tr.position = new Vector3(pos.x + panGesture.deltaPos.x * moveFactor, pos.y, pos.z + panGesture.deltaPos.y * moveFactor);

            var magGesture = Platform.magnificationGesture;
            if (magGesture.phase == TouchPhase.Moved)
            {
                var scale = tr.localScale.x + magGesture.deltaMagnification * scaleFactor;
                scale = Mathf.Clamp(scale, 0.001f, 100f);
                tr.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
