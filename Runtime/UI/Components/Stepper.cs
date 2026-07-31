using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A control that lets users incrementally adjust a value by pressing the plus or minus buttons.
    /// </summary>
    /// <remarks>
    /// The Stepper component provides a simple way to increment or decrement numeric values through button
    /// interactions. It consists of two buttons: one for increasing and one for decreasing the value.
    ///
    /// Steppers are particularly useful when users need to make small adjustments to a value, such as changing
    /// quantity, adjusting volume, or modifying numeric settings.
    ///
    /// The component supports both mouse/touch interaction and keyboard navigation. Users can interact with the
    /// stepper using:
    /// - Plus/Minus buttons
    /// - Left/Right arrow keys
    /// - Keyboard Plus/Minus keys
    ///
    /// Note: The stepper's value is represented as -1 for decrement and 1 for increment, making it ideal for
    /// relative value adjustments rather than absolute values.
    /// </remarks>
    /// <example>
    /// <para>Basic Stepper Usage: creating a basic stepper to manage a counter value.</para>
    /// <code lang="csharp"><![CDATA[
    /// var stepper = new Stepper();
    /// var count = 0;
    ///
    /// stepper.RegisterValueChangedCallback(evt => {
    ///     count += evt.newValue;
    ///     Debug.Log($"Current count: {count}");
    /// });
    ///
    /// container.Add(stepper);
    /// ]]></code>
    /// <para>Customized Stepper with Size: creating a large stepper with custom styling in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:ui="Unity.AppUI.UI">
    ///     <ui:Stepper size="L" class="custom-stepper" />
    /// </UXML>
    /// ]]></code>
    /// <para>Stepper with Value Change Handling: implementing a volume control using a stepper with clamped values.</para>
    /// <code lang="csharp"><![CDATA[
    /// public class VolumeControl : VisualElement
    /// {
    ///     private float volume = 50f;
    ///     private const float STEP = 5f;
    ///
    ///     public VolumeControl()
    ///     {
    ///         var stepper = new Stepper();
    ///         stepper.RegisterValueChangedCallback(evt => {
    ///             volume = Mathf.Clamp(volume + (STEP * evt.newValue), 0f, 100f);
    ///             Debug.Log($"Volume adjusted to: {volume}%");
    ///         });
    ///         Add(stepper);
    ///     }
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("actions")]
    public partial class Stepper : ExVisualElement, INotifyValueChanged<int>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId sizeProperty = nameof(size);


        /// <summary>
        /// The Stepper main styling class.
        /// </summary>
        public const string ussClassName = "appui-stepper";

        /// <summary>
        /// The Stepper size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Stepper increment icon styling class.
        /// </summary>
        public const string incIconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Stepper decrement icon styling class.
        /// </summary>
        public const string decIconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// The Stepper increment icon container styling class.
        /// </summary>
        public const string incIconContainerUssClassName = ussClassName + "__iconcontainer";

        /// <summary>
        /// The Stepper decrement icon container styling class.
        /// </summary>
        public const string decIconContainerUssClassName = ussClassName + "__iconcontainer";

        /// <summary>
        /// The Stepper decrement button styling class.
        /// </summary>
        public const string decButtonUssClassName = ussClassName + "__decbutton";

        /// <summary>
        /// The Stepper increment button styling class.
        /// </summary>
        public const string incButtonUssClassName = ussClassName + "__incbutton";

        /// <summary>
        /// The Stepper general button styling class.
        /// </summary>
        public const string buttonUssClassName = ussClassName + "__button";

        Size m_Size;

        int m_Value;

        readonly Pressable m_DecClickable;

        readonly Pressable m_IncClickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Stepper()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;

            var decIcon = new Icon { name = decIconUssClassName, iconName = "minus", pickingMode = PickingMode.Ignore };
            decIcon.AddToClassList(decIconUssClassName);
            var incIcon = new Icon { name = incIconUssClassName, iconName = "plus", pickingMode = PickingMode.Ignore };
            incIcon.AddToClassList(incIconUssClassName);

            var decIconContainer = new VisualElement { name = decIconContainerUssClassName, pickingMode = PickingMode.Ignore };
            decIconContainer.AddToClassList(decIconContainerUssClassName);
            var incIconContainer = new VisualElement { name = incIconContainerUssClassName, pickingMode = PickingMode.Ignore };
            incIconContainer.AddToClassList(incIconContainerUssClassName);

            var decButton = new VisualElement { name = decButtonUssClassName };
            decButton.AddToClassList(buttonUssClassName);
            decButton.AddToClassList(decButtonUssClassName);
            var incButton = new VisualElement { name = incButtonUssClassName };
            incButton.AddToClassList(Styles.lastChildUssClassName); // todo use :last-child state in the USS when available
            incButton.AddToClassList(buttonUssClassName);
            incButton.AddToClassList(incButtonUssClassName);

            decIconContainer.hierarchy.Add(decIcon);
            incIconContainer.hierarchy.Add(incIcon);

            decButton.hierarchy.Add(decIconContainer);
            incButton.hierarchy.Add(incIconContainer);

            hierarchy.Add(decButton);
            hierarchy.Add(incButton);

            m_DecClickable = new Pressable(OnDecrementClicked);
            decButton.AddManipulator(m_DecClickable);
            m_IncClickable = new Pressable(OnIncrementClicked);
            incButton.AddManipulator(m_IncClickable);

            RegisterCallback<KeyDownEvent>(OnKeyDown);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            size = Size.M;
        }

        /// <summary>
        /// The content container of the Stepper. Always null.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The size of the Stepper.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// Set the value of the Stepper without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(int newValue)
        {
            m_Value = newValue;
        }

        /// <summary>
        /// <para>The value of the Stepper. 1 means increment, -1 means decrement.</para>
        /// <para>
        /// It is not recommended to get or set this value directly.
        /// To track the changes of the value, use <see cref="INotifyValueChangedExtensions.RegisterValueChangedCallback{T}"/> instead.
        /// </para>
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int value
        {
            get => m_Value;
            set
            {
                using var evt = ChangeEvent<int>.GetPooled(0, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target == this)
            {
                var handled = false;

                if (evt.keyCode == KeyCode.Plus || evt.keyCode == KeyCode.KeypadPlus || evt.keyCode == KeyCode.RightArrow)
                {
                    m_IncClickable.SimulateSingleClickInternal(evt);
                    handled = true;
                }
                else if (evt.keyCode == KeyCode.Minus || evt.keyCode == KeyCode.KeypadMinus || evt.keyCode == KeyCode.LeftArrow)
                {
                    m_DecClickable.SimulateSingleClickInternal(evt);
                    handled = true;
                }

                if (handled)
                {
                    evt.StopPropagation();

                }
            }
        }

        void OnIncrementClicked()
        {
            value = 1;
        }

        void OnDecrementClicked()
        {
            value = -1;
        }

    }
}
