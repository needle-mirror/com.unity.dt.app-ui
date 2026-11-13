#if APPUI_UITF_PRESENT
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.TestFramework;
using AppUIPanel = Unity.AppUI.UI.Panel;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Base fixture for the interaction half of a component's tests, built on
    /// <c>com.unity.ui.test-framework</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This does not replace <see cref="VisualElementTests{T}"/>, which stays the story and
    /// snapshot harness and keeps owning construction and UXML coverage for every component. The
    /// two have incompatible needs -- snapshots want a rendered <c>UIDocument</c> panel at a pinned
    /// resolution, interaction wants a cheap panel and deterministic time -- and duplicating the
    /// constructor and UXML tests here would only double them across the editor matrix.
    /// </para>
    /// <para>
    /// What the framework buys us is real input and real frames.
    /// <see cref="AbstractUITestFixture.simulate"/> dispatches through the actual event system, and
    /// <c>simulate.FrameUpdate()</c> runs the true UI Toolkit update loop while advancing simulated
    /// time, so a test no longer has to guess at a number of <c>yield return null</c>s or fake the
    /// clock with <c>TimeUtils.timeOverride</c>.
    /// </para>
    /// <para>
    /// Input goes through <c>simulate</c> directly -- <c>simulate.Click</c>,
    /// <c>simulate.MouseDown</c>, <c>simulate.DragAndDrop</c>, <c>simulate.ReturnKeyPress</c> and
    /// friends. Those build an IMGUI <c>Event</c>, but UI Toolkit turns it into a real
    /// <c>PointerDownEvent</c>/<c>PointerUpEvent</c> pair carrying the mouse pointer id before the
    /// matching mouse event, so App UI's <c>Pressable</c> and <c>Draggable</c> both receive what
    /// they listen for. Verified against ActionButton, TouchSlider and the SplitView splitter.
    /// </para>
    /// <para>
    /// Positions come from <c>worldBound</c>, so an element has to be laid out before it can be
    /// clicked -- <c>Pressable</c> only raises <c>clicked</c> when the release lands inside the
    /// target's bounds, and <c>Draggable</c> ignores a drag shorter than its 4px threshold. Use
    /// <see cref="MountElement()"/>, which settles layout before returning.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The component under test.</typeparam>
    class UIComponentTests<T> : UITestFixture where T : VisualElement, new()
    {
        /// <summary>
        /// The panel size used by the fixture. Larger than the framework's 500x500 default so wide
        /// components lay out fully rather than getting clipped, which would move their
        /// <c>worldBound</c> and break position-based input.
        /// </summary>
        /// <remarks>
        /// Clamped to the screen, which is what actually bounds pointer input. <c>simulate</c>
        /// builds an IMGUI <c>Event</c> in screen space, so a click is only delivered where the
        /// screen reaches -- a panel wider than the screen lays out fine and reports a valid
        /// <c>worldBound</c>, but everything past the screen edge silently receives no pointer
        /// event at all. A batchmode editor runs at 640x480, so an unclamped 1200-wide panel put
        /// every right-aligned control (a field's picker icon, a SplitButton's caret, a card's
        /// close button) out of reach, and the tests for them failed as though the component were
        /// broken. Clamping keeps the whole panel clickable, and a host with a larger screen still
        /// gets the full 1200x600.
        /// </remarks>
        protected virtual Vector2 testPanelSize => new Vector2(
            Mathf.Min(1200f, Screen.width),
            Mathf.Min(600f, Screen.height));

        /// <summary>
        /// The App UI <c>Panel</c> hosting the element under test. App UI components resolve their
        /// context -- theme, scale, layout direction, language -- by walking up to a <c>Panel</c>,
        /// so a component parented straight onto
        /// <see cref="AbstractUITestFixture.rootVisualElement"/> would be unstyled and would behave
        /// differently from one in a real app.
        /// </summary>
        protected AppUIPanel appUiPanel { get; private set; }

        /// <summary>
        /// The element under test, set by <see cref="MountElement()"/>.
        /// </summary>
        protected T element { get; private set; }

        /// <inheritdoc cref="AbstractUITestFixture.FixtureUnitySetUp"/>
        public override IEnumerator FixtureUnitySetUp()
        {
            // AbstractUITestFixture carries the NUnit attributes on its Fixture* virtuals, so these
            // overrides compose with the base lifecycle rather than replacing it, and the base call
            // always comes first.
            yield return base.FixtureUnitySetUp();

            // Only the coroutine work belongs here. Anything touching the panel has to wait for
            // FixtureSetUp -- see below.
            yield return Utils.WaitForLocalizationPreloaded();
        }

        /// <inheritdoc cref="AbstractUITestFixture.FixtureSetUp"/>
        /// <remarks>
        /// The panel is built here rather than in <see cref="FixtureUnitySetUp"/> because
        /// <c>[UnitySetUp]</c> runs first, while the fixture is still merely initialised.
        /// <c>FixtureSetUp</c> is what moves it into its during-test state, and
        /// <c>simulate.FrameUpdate()</c> throws <c>"FrameUpdate can only be called during a test"</c>
        /// before that. Per-test is also the right granularity: <c>clearContentAfterTest</c>
        /// defaults to <c>true</c>, so the root is emptied after every test case anyway.
        /// </remarks>
        public override void FixtureSetUp()
        {
            base.FixtureSetUp();

            // Order matters. themeStyleSheet's setter only stores the value -- the fixture applies it
            // when it builds a panel -- and the panel already exists by now, created during
            // FixtureOneTimeSetUp. Assigning it and carrying on leaves the panel on the default
            // theme, and an App UI Panel with no App UI theme resolves to zero height: it stretches
            // across the root's width but has nothing to give it a height, so every child collapses
            // to a zero-height rect, every position-based pointer event misses, and an element like
            // SplitView never fires the GeometryChangedEvent its splitters are built from. Set the
            // theme, then rebuild the panel so it is actually used.
            themeStyleSheet = Utils.appUiThemeStyleSheet;
            RecreatePanel();

            // After RecreatePanel, since recreating resets the panel to its default size.
            panelSize = testPanelSize;
            pixelsPerPoint = 1f;

            // Never true in CI. With debugMode on, a failure suspends cleanup -- it leaves the panel
            // alive and can set EditorApplication.isPaused, which blocks FixtureUnityTearDown and
            // cascades the failure into every test after it in a batch-mode run.
            debugMode = false;

            appUiPanel = new AppUIPanel();
            rootVisualElement.Add(appUiPanel);
            simulate.FrameUpdate();
        }

        /// <inheritdoc cref="AbstractUITestFixture.FixtureTearDown"/>
        public override void FixtureTearDown()
        {
            appUiPanel = null;
            element = null;
            base.FixtureTearDown();
        }

        /// <summary>
        /// Parents a new instance of <typeparamref name="T"/> under <see cref="appUiPanel"/> and
        /// lets it lay out, so its <c>worldBound</c> is valid for position-based input.
        /// </summary>
        /// <returns>The mounted element, also available as <see cref="element"/>.</returns>
        protected T MountElement() => MountElement(new T());

        /// <summary>
        /// Parents <paramref name="instance"/> under <see cref="appUiPanel"/> and lets it lay out.
        /// </summary>
        /// <param name="instance">The element to mount.</param>
        /// <returns>The mounted element, also available as <see cref="element"/>.</returns>
        protected T MountElement(T instance)
        {
            appUiPanel.Clear();
            appUiPanel.Add(instance);
            element = instance;

            // Two passes: the first resolves styles, the second settles any layout the resolved
            // styles themselves changed.
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            // Positive control. Every pointer helper here is position-based, so an element with no
            // resolved layout silently swallows all input and the negative tests ("nothing should
            // happen when disabled") pass for entirely the wrong reason. Fail at the mount instead.
            Assert.Greater(instance.worldBound.width, 0f,
                $"{typeof(T).Name} has no resolved layout, so no pointer input can reach it.");
            Assert.Greater(instance.worldBound.height, 0f,
                $"{typeof(T).Name} has no resolved layout, so no pointer input can reach it.");

            return instance;
        }

    }
}
#endif
