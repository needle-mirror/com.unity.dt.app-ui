using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Popover))]
    class PopoverTests : AnchorPopupTests<Popover>
    {
        protected override AnchorPopup<Popover> CreatePopup()
        {
            return Popover.Build(GetReferenceElement(), GetContentElement());
        }

        protected override void OnCanBuildPopupTested()
        {
            base.OnCanBuildPopupTested();

            var popover = (Popover)popup;

            Assert.IsNotNull(popover);
            Assert.IsNotNull(popover.anchor);
            Assert.AreEqual(0, popover.offset);
            Assert.IsTrue(popover.arrowVisible);
            Assert.AreEqual(0, popover.containerPadding);
            Assert.AreEqual(0, popover.crossOffset);
            Assert.AreEqual(PopoverPlacement.Bottom, popover.currentPlacement);
            Assert.IsTrue(popover.shouldFlip);
            Assert.IsTrue(popover.outsideClickDismissEnabled);

            Assert.IsFalse(popover.modalBackdrop);
            popover.SetModalBackdrop(true);
            Assert.IsTrue(popover.modalBackdrop);

            Assert.IsNotNull(popover.view.contentContainer);

            Assert.AreEqual(PopoverPlacement.Bottom, popover.placement);
            popover.SetPlacement(PopoverPlacement.Left);
            Assert.AreEqual(PopoverPlacement.Left, popover.placement);
            popover.SetPlacement(PopoverPlacement.Right);
            Assert.AreEqual(PopoverPlacement.Right, popover.placement);
        }

        [UnityTest]
        public IEnumerator FocusDelegation_IgnoresFocusableElementsInsideHiddenParents()
        {
            // A Dialog's ScrollView hides its scrollers, whose controls are still focusable. Delegating
            // focus to one of them resolves to nothing, which blurs the panel and dismisses the popover.
            var hidden = new VisualElement { style = { display = DisplayStyle.None } };
            hidden.Add(new VisualElement { focusable = true });
            var content = new VisualElement();
            content.Add(hidden);

            var view = new Popover.PopoverVisualElement(content);
            // The hidden parent is known from its inline style before any layout, so the only
            // focusable content is rejected and focus falls back to the popover element itself.
            Assert.IsFalse(view.popoverElement.delegatesFocus,
                "A control under a display:none parent must not make the popover delegate focus.");

            GetReferenceElement().parent.Add(view);
            yield return null;

            Assert.IsNull(view.RefreshDelegatesFocus());
            Assert.IsFalse(view.popoverElement.delegatesFocus);

            var visible = new VisualElement { focusable = true };
            content.Add(visible);
            yield return null;

            Assert.AreSame(visible, view.RefreshDelegatesFocus());
            Assert.IsTrue(view.popoverElement.delegatesFocus);

            view.RemoveFromHierarchy();
        }
    }
}
