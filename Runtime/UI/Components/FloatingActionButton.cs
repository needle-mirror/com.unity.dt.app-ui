using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A floating action button.
    /// </summary>
    public class FloatingActionButton : ExVisualElement, IPressable
    {
        /// <summary>
        /// The Floating Action Button's USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-fab";
        
        /// <summary>
        /// The Floating Action Button's size USS class name.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";
        
        Pressable m_Clickable;

        int m_Elevation;

        Size m_Size;

        /// <summary>
        /// The clickable manipulator used by this button.
        /// </summary>
        public Pressable clickable
        {
            get => m_Clickable;
            private set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }
        
        /// <summary>
        /// Event fired when the button is clicked.
        /// </summary>
        public event Action clicked
        {
            add => clickable.clicked += value;
            remove => clickable.clicked -= value;
        }

        /// <summary>
        /// The elevation of this element.
        /// </summary>
        public int elevation
        {
            get => m_Elevation;
            set
            {
                RemoveFromClassList(Styles.elevationUssClassName + m_Elevation);
                m_Elevation = value;
                AddToClassList(Styles.elevationUssClassName + m_Elevation);
            }
        }
        
        /// <summary>
        /// The size of this element.
        /// </summary>
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());
            }
        }

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FloatingActionButton() : this(null) { }
        
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="clickAction"> The action to perform when the button is clicked. </param>
        public FloatingActionButton(Action clickAction)
        {
            AddToClassList(ussClassName);
            
            clickable = new Pressable(clickAction);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            passMask = Passes.Clear | Passes.OutsetShadows;
            elevation = 12;
            size = Size.M;
            
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnPointerFocus));
        }

        void OnPointerFocus(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.OutsetShadows;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.OutsetShadows | Passes.Outline;
        }
        
        /// <summary>
        /// Whether the element is disabled.
        /// </summary>
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }
        
        /// <summary>
        /// Defines the UxmlFactory for the <see cref="FloatingActionButton"/>.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<FloatingActionButton, UxmlTraits> { }

        /// <summary>
        /// Class containing the UXML traits for the <see cref="FloatingActionButton"/>.
        /// </summary>
        public new class UxmlTraits : ExVisualElement.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
            };
            
            readonly UxmlIntAttributeDescription m_Elevation = new UxmlIntAttributeDescription
            {
                name = "elevation",
                defaultValue = 12,
            };
            
            readonly UxmlBoolAttributeDescription m_Disabled = new UxmlBoolAttributeDescription
            {
                name = "disabled",
                defaultValue = false,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var element = (FloatingActionButton)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.elevation = m_Elevation.GetValueFromBag(bag, cc);
                
                element.disabled = m_Disabled.GetValueFromBag(bag, cc);
            }
        }
    }
}
