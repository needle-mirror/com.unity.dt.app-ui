#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.Tests.UI
{
    // Keyboard focus opens a control's tooltip and blur closes it, like the registry's Radix
    // trigger. Focus from a pointer press does not, and a disabled control gets no focus tooltip.
    [TestFixture]
    [TestOf(typeof(TooltipManipulator))]
    class TooltipManipulatorBehaviourTests : UIComponentTests<Button>
    {
        Button m_First;
        Button m_Second;

        void MountButtons()
        {
            appUiPanel.forceUseTooltipSystem = true;
            appUiPanel.tooltipDelayMs = 0;
            m_First = MountElement(new Button { title = "First", tooltip = "First tooltip" });
            m_Second = new Button { title = "Second", tooltip = "Second tooltip" };
            m_First.parent.Add(m_Second);
            Settle();
        }

        void Settle()
        {
            for (var i = 0; i < 4; i++)
                simulate.FrameUpdate();
        }

        string ShownText()
        {
            var shown = appUiPanel.tooltipContainer.Q<LocalizedTextElement>(className: "appui-tooltip__content");
            return shown?.text;
        }

        [Test]
        public void Focus_OpensTheTooltip()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            Assert.AreEqual("First tooltip", ShownText(), "Keyboard focus should open the control's tooltip.");
        }

        [Test]
        public void Blur_ClosesTheTooltip()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            Assert.AreEqual("First tooltip", ShownText());

            m_First.Blur();
            Settle();
            Assert.IsNull(ShownText(), "Blurring the control should close its focus tooltip.");
        }

        [Test]
        public void MovingFocus_ShowsTheNextControlsTooltip()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            m_Second.Focus();
            Settle();
            Assert.AreEqual("Second tooltip", ShownText(), "Focus moving on should show the next tooltip.");
        }

        [Test]
        public void Escape_DismissesTheFocusTooltip()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            Assert.AreEqual("First tooltip", ShownText());

            simulate.KeyPress(KeyCode.Escape);
            Settle();
            Assert.IsNull(ShownText(), "Escape should dismiss the tooltip.");
        }

        [Test]
        public void HoverAfterEscapingAFocusTooltip_OpensItAgain()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            simulate.KeyPress(KeyCode.Escape);
            Settle();
            Assert.IsNull(ShownText());

            simulate.MouseMove(m_First.worldBound.center);
            Settle();
            Assert.AreEqual("First tooltip", ShownText(),
                "A dismissed focus tooltip must not block the next hover on the same control.");
        }

        [Test]
        public void HoveringAnotherControl_ReplacesTheFocusTooltip()
        {
            MountButtons();
            m_First.Focus();
            Settle();
            Assert.AreEqual("First tooltip", ShownText());

            simulate.MouseMove(m_Second.worldBound.center);
            Settle();
            Assert.AreEqual("Second tooltip", ShownText(),
                "One move onto another control should swap the focus tooltip for its hover tooltip.");
        }

        [Test]
        public void FocusInANestedPanel_OpensOnlyThatPanelsTooltip()
        {
            MountButtons();
            var inner = new Panel { forceUseTooltipSystem = true, tooltipDelayMs = 0, style = { height = 100 } };
            var innerButton = new Button { title = "Inner", tooltip = "Inner tooltip" };
            inner.Add(innerButton);
            m_First.parent.Add(inner);
            Settle();

            innerButton.Focus();
            Settle();
            Assert.IsNull(ShownText(), "The outer panel must not answer focus inside a nested panel.");
            var innerShown = inner.tooltipContainer.Q<LocalizedTextElement>(className: "appui-tooltip__content");
            Assert.AreEqual("Inner tooltip", innerShown?.text, "The nested panel shows its own control's tooltip.");
        }

        [Test]
        public void PointerClick_ThatFocuses_DoesNotOpenTheTooltip()
        {
            MountButtons();
            simulate.Click(m_First);
            Assert.AreEqual(m_First, appUiPanel.focusController.focusedElement,
                "The click should have focused the button for this test to mean anything.");
            Settle();
            Assert.IsNull(ShownText(), "Focus given by a pointer press should not open a tooltip.");
        }

        [Test]
        public void KeyboardAfterAPressWhoseReleaseNeverArrived_OpensTheFocusTooltip()
        {
            // A captured drag (a Draggable slider, a Canvas pan) can stop the release before the root
            // sees it. The next keyboard input must still give keyboard focus its tooltip.
            MountButtons();
            using (var down = PointerDownEvent.GetPooled())
            {
                down.target = m_First;
                m_First.SendEvent(down);
            }
            Settle();

            simulate.KeyPress(KeyCode.Tab);
            m_Second.Focus();
            Settle();
            Assert.AreEqual("Second tooltip", ShownText(),
                "A lost pointer release must not suppress keyboard focus tooltips for good.");
        }

        [Test]
        public void DisabledControl_GetsNoFocusTooltip()
        {
            MountButtons();
            m_First.SetEnabled(false);
            m_First.Focus();
            Settle();
            Assert.IsNull(ShownText(), "A disabled control gets no focus tooltip.");
        }

        [Test]
        public void FocusInOnADisabledControl_DoesNotOpenTheTooltip()
        {
            // UI Toolkit will not focus a disabled element, so send the focus event directly to
            // prove the manipulator's own guard holds if one ever arrives.
            MountButtons();
            m_First.SetEnabled(false);
            Settle();
            using (var evt = FocusInEvent.GetPooled(m_First, null, FocusChangeDirection.unspecified, appUiPanel.focusController))
                m_First.SendEvent(evt);
            Settle();
            Assert.IsNull(ShownText(), "A disabled control gets no focus tooltip.");
        }

        [Test]
        public void DisabledControl_KeepsItsHoverTooltip()
        {
            MountButtons();
            m_First.SetEnabled(false);
            Settle();
            simulate.MouseMove(m_First.worldBound.center);
            Settle();
            Assert.AreEqual("First tooltip", ShownText(), "A disabled control keeps its hover tooltip.");
        }

        [Test]
        public void Hover_StillOpensTheTooltip()
        {
            MountButtons();
            simulate.MouseMove(m_Second.worldBound.center);
            Settle();
            Assert.AreEqual("Second tooltip", ShownText(), "Pointer hover behaviour is unchanged.");
        }
    }
}
#endif
