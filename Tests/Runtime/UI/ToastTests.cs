using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toast))]
    class ToastTests : PopupNotificationTests<Toast>
    {
        protected override Toast CreatePopup()
        {
            return Toast.Build(GetReferenceElement(), "Hello", NotificationDuration.Indefinite);
        }

        protected override bool shouldContainView => false;

        const string k_Action = ".appui-toast .appui-toast__actioncontainer > .appui-toast__action";

        /// <summary>
        /// A hovered toast action fills with the hover of a transparent action, as the registry's
        /// ToastAction does, rather than a raw palette step. The package drives hover through the
        /// <c>is-hovered</c> class, so that selector is the one that has to carry it.
        /// </summary>
        /// <remarks>
        /// That fill is opaque, and the coloured variants paint their action text white, which it
        /// would wash out in a light theme. So the hover also takes the matching quiet action
        /// foreground, and its selector carries the extra <c>.appui-toast</c> class that outranks the
        /// three-class variant rules.
        /// </remarks>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_ActionHover_ReadsTheQuietActionHoverToken()
        {
            var uss = UssSource.Read("Toast.uss");

            foreach (var selector in new[] { k_Action + ".is-hovered", k_Action + ":hover" })
            {
                var block = UssSource.RuleBlock(uss, selector);
                Assert.IsNotNull(block, $"Toast.uss has no {selector} rule.");
                StringAssert.Contains("background-color: var(--appui-alias-actions-quiet-background-color-hover)", block,
                    $"{selector} must fill with the quiet action hover token.");
                StringAssert.Contains("color: var(--appui-alias-actions-quiet-foreground-color-hover)", block,
                    $"{selector} must pair the opaque hover fill with its readable foreground.");
                Assert.IsFalse(block.Contains("--appui-color-transparent-black-"),
                    $"{selector} reaches for a raw palette step instead of the action hover alias.");
            }
        }

        /// <summary>
        /// A press adds <c>is-active</c> without removing <c>is-hovered</c>. The pressed rule must carry
        /// the same specificity as the hover rule and come after it, or a press keeps the hover fill
        /// and loses its feedback. It pairs the quiet action fill and foreground the same way.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_ActionActive_OutranksTheHoverRule()
        {
            var uss = UssSource.Read("Toast.uss");

            foreach (var selector in new[] { k_Action + ".is-active", k_Action + ":active" })
            {
                var block = UssSource.RuleBlock(uss, selector);
                Assert.IsNotNull(block, $"Toast.uss has no {selector} rule.");
                StringAssert.Contains("background-color: var(--appui-alias-actions-quiet-background-color-active)", block);
                StringAssert.Contains("color: var(--appui-alias-actions-quiet-foreground-color-active)", block);
            }

            Assert.Greater(uss.IndexOf(k_Action + ".is-active", System.StringComparison.Ordinal),
                uss.IndexOf(k_Action + ".is-hovered", System.StringComparison.Ordinal),
                "The pressed rule must come after the hover rule so it wins while both classes are set.");
        }
    }
}
