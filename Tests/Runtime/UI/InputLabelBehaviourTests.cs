#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(InputLabel))]
    class InputLabelBehaviourTests : UIComponentTests<InputLabel>
    {
        /// <summary>
        /// A stand-in for a scrubbable numeric field: it only records the <see cref="DragContext"/>
        /// phases its ancestor <see cref="InputLabel"/> provides, the same way
        /// <c>NumericalField</c> consumes them.
        /// </summary>
        class DragContextListener : VisualElement
        {
            public readonly List<DragPhase> phases = new();

            public DragContextListener()
            {
                this.RegisterContextChangedCallback<DragContext>(OnDragContextChanged);
            }

            void OnDragContextChanged(ContextChangedEvent<DragContext> evt)
            {
                if (evt.context != null)
                    phases.Add(evt.context.phase);
            }
        }

        InputLabel MountDraggableInputLabel(out DragContextListener listener)
        {
            var inputLabel = new InputLabel("Opacity") { draggable = true, style = { width = 300 } };
            listener = new DragContextListener { style = { width = 100, height = 20 } };
            inputLabel.Add(listener);
            return MountElement(inputLabel);
        }

        [Test]
        public void InputLabel_ShiftDragOnLabel_ShouldSendDragContextToChildren()
        {
            var inputLabel = MountDraggableInputLabel(out var listener);
            var labelContainer = inputLabel.Q(className: InputLabel.labelContainerUssClassName);
            Assert.IsNotNull(labelContainer, "The InputLabel should have a label container to drag from.");

            var from = labelContainer.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(20f, 0f), UnityEngine.UIElements.MouseButton.LeftMouse,
                EventModifiers.Shift);
            simulate.FrameUpdate();

            CollectionAssert.Contains(listener.phases, DragPhase.Started,
                "A shift-drag on the label should start a DragContext for descendants.");
            CollectionAssert.Contains(listener.phases, DragPhase.Dragging,
                "Moving during the drag should send a Dragging phase.");
        }

        [Test]
        public void InputLabel_WhenNotDraggable_DragOnLabel_ShouldNotSendDragContext()
        {
            var inputLabel = new InputLabel("Opacity") { draggable = false, style = { width = 300 } };
            var listener = new DragContextListener { style = { width = 100, height = 20 } };
            inputLabel.Add(listener);
            MountElement(inputLabel);

            var labelContainer = inputLabel.Q(className: InputLabel.labelContainerUssClassName);
            var from = labelContainer.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(20f, 0f), UnityEngine.UIElements.MouseButton.LeftMouse,
                EventModifiers.Shift);
            simulate.FrameUpdate();

            Assert.IsEmpty(listener.phases, "With draggable false, the label should not carry a Draggable manipulator.");
        }

        VisualElement LabelOf(InputLabel row) => row.Q(className: InputLabel.labelContainerUssClassName);

        InputLabel MountLabelledRow(VisualElement control, bool draggable = false)
        {
            control.name = "the-control";
            var row = new InputLabel("Enabled") { labelFor = "the-control", draggable = draggable, style = { width = 300 } };
            row.Add(control);
            return MountElement(row);
        }

        [Test]
        public void InputLabel_ClickOnLabel_TogglesTheCheckboxItLabels()
        {
            var checkbox = new Checkbox();
            var row = MountLabelledRow(checkbox);
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value);

            simulate.Click(LabelOf(row));
            Assert.AreEqual(CheckboxState.Checked, checkbox.value, "A click on the label must operate the checkbox.");

            simulate.Click(LabelOf(row));
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value);
        }

        [Test]
        public void InputLabel_ClickOnLabel_TogglesTheToggleItLabels()
        {
            var toggle = new Unity.AppUI.UI.Toggle();
            var row = MountLabelledRow(toggle);
            Assert.IsFalse(toggle.value);

            simulate.Click(LabelOf(row));
            Assert.IsTrue(toggle.value, "A click on the label must operate the toggle.");
        }

        [Test]
        public void InputLabel_WithoutLabelFor_ClickOnLabel_LeavesTheControlAlone()
        {
            var toggle = new Unity.AppUI.UI.Toggle();
            var row = new InputLabel("Enabled") { style = { width = 300 } };
            row.Add(toggle);
            MountElement(row);

            simulate.Click(LabelOf(row));
            Assert.IsFalse(toggle.value, "The label only operates a control it names, like <label for>.");
        }

        [Test]
        public void InputLabel_WhenDraggable_ClickOnLabel_DoesNotToggle()
        {
            var toggle = new Unity.AppUI.UI.Toggle();
            var row = MountLabelledRow(toggle, draggable: true);

            simulate.Click(LabelOf(row));
            Assert.IsFalse(toggle.value,
                "A scrubbable label must not toggle: a drag ending on it would flip the control every scrub.");
        }

        [Test]
        public void InputLabel_ClickOnLabel_LeavesADisabledControlAlone()
        {
            var checkbox = new Checkbox();
            checkbox.SetEnabled(false);
            var row = MountLabelledRow(checkbox);

            simulate.Click(LabelOf(row));
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value, "A disabled control must not be operated by its label.");
        }

        /// <summary>
        /// As with an HTML label, the click hands focus to the control it operates, here one outside the
        /// row, so the keyboard carries on where the click left off.
        /// </summary>
        [Test]
        public void InputLabel_ClickOnLabel_FocusesTheControlItLabels()
        {
            var row = MountElement(new InputLabel("Enabled") { labelFor = "remote-checkbox", style = { width = 300 } });
            var checkbox = new Checkbox { name = "remote-checkbox" };
            row.parent.Add(checkbox);
            simulate.FrameUpdate();

            simulate.Click(LabelOf(row));
            Assert.AreEqual(CheckboxState.Checked, checkbox.value, "The label must operate a control it names outside the row.");
            Assert.AreSame(checkbox, checkbox.focusController?.focusedElement, "The labelled control must take focus.");
        }

        [Test]
        public void InputLabel_SetLabel_ShouldUpdateFieldLabelText()
        {
            var inputLabel = MountElement(new InputLabel { label = "Initial" });

            inputLabel.label = "Updated";

            var fieldLabel = inputLabel.Q<FieldLabel>();
            Assert.AreEqual("Updated", fieldLabel.label, "Setting label should update the underlying FieldLabel.");
        }
    }
}
#endif
