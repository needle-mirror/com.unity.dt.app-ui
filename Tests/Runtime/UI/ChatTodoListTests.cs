using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatTodoList))]
    class ChatTodoListTests : VisualElementTests<ChatTodoList>
    {
        const string k_Uss = "ChatTodoList.uss";

        protected override string mainUssClassName => ChatTodoList.ussClassName;

        // Handed over to ChatTodoListSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static ChatTodoItem[] SampleItems() => new[]
        {
            new ChatTodoItem("Read the player controller", ChatTodoStatus.Completed),
            new ChatTodoItem("Raise the jump force", ChatTodoStatus.InProgress),
            new ChatTodoItem("Re-run the tests"),
            new ChatTodoItem("Ship it", ChatTodoStatus.Cancelled),
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Open", (ctx) => new ChatTodoList { items = SampleItems() });
                yield return new Story("Collapsed", (ctx) => new ChatTodoList { items = SampleItems(), open = false });
                yield return new Story("CustomTitle", (ctx) => new ChatTodoList { items = SampleItems(), title = "Plan" });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatTodoList />",
            @"<appui:ChatTodoList title=""Plan"" open=""false"" />",
            @"<appui:ChatTodoList completed-tone=""Accent"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var list = new ChatTodoList();
            Assert.AreEqual(ChatTodoList.defaultTitle, list.title);
            Assert.IsTrue(list.open, "The checklist starts expanded, like the registry's defaultOpen.");
        }

        /// <summary>
        /// The failure this pins is silent: the source generator lowercases the whole member name, so
        /// InProgress becomes "inprogress". A USS file written as "--status-in-progress" would style
        /// nothing at all, with no error.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void StatusClasses_MatchWhatTheGeneratorEmitsAndWhatTheUssDefines()
        {
            var list = new ChatTodoList { items = SampleItems() };
            var rows = list.Query(className: ChatTodoList.itemUssClassName).ToList();

            Assert.AreEqual(4, rows.Count);
            Assert.IsTrue(rows[0].ClassListContains("appui-chat-todo-list__item--status-completed"));
            Assert.IsTrue(rows[1].ClassListContains("appui-chat-todo-list__item--status-inprogress"));
            Assert.IsTrue(rows[2].ClassListContains("appui-chat-todo-list__item--status-pending"));
            Assert.IsTrue(rows[3].ClassListContains("appui-chat-todo-list__item--status-cancelled"));

            var uss = ChatTestUtils.ReadUss(k_Uss);
            foreach (var name in new[] { "completed", "inprogress", "cancelled" })
            {
                StringAssert.Contains($".appui-chat-todo-list__item--status-{name}", uss,
                    $"ChatTodoList.uss defines no rule for the '{name}' status the generator emits.");
            }
        }

        [Test]
        public void Tally_CountsOnlyCompletedItems()
        {
            var list = new ChatTodoList { items = SampleItems() };
            Assert.AreEqual(4, list.itemCount);
            Assert.AreEqual(1, list.completedCount,
                "Cancelled is terminal but is not a success, so it must not count as done.");
        }

        [Test]
        public void EachStatus_GetsItsOwnGlyph()
        {
            var list = new ChatTodoList { items = SampleItems() };
            var rows = list.Query(className: ChatTodoList.itemUssClassName).ToList();

            Assert.IsInstanceOf<Icon>(rows[0][0][0], "Completed is a check-circle icon.");
            Assert.IsTrue(rows[1][0][0].ClassListContains(ChatTodoList.inProgressGlyphUssClassName),
                "In-progress is a static filled ring, deliberately not a spinner: a checklist shows "
                + "several rows at once and a spinning one pulls the eye off the text.");
            Assert.IsNotInstanceOf<CircularProgress>(rows[1][0][0]);
            Assert.IsTrue(rows[2][0][0].ClassListContains(ChatTodoList.pendingGlyphUssClassName),
                "Pending is an empty outline circle, not an icon.");
            Assert.IsInstanceOf<Icon>(rows[3][0][0], "Cancelled is an x icon.");
        }

        [Test]
        public void Open_TogglesTheItemsAndTheCaret()
        {
            var list = new ChatTodoList { items = SampleItems() };
            var items = list.Q(ChatTodoList.itemsUssClassName);
            Assert.IsFalse(items.ClassListContains("unity-hidden"));

            list.open = false;
            Assert.IsTrue(items.ClassListContains("unity-hidden"));
            Assert.IsFalse(list.ClassListContains("is-open"),
                "The open marker must come off with the items, so styling can key on it.");
        }

        [Test]
        public void EmptyList_HidesItselfRatherThanShowingAnEmptyBox()
        {
            var list = new ChatTodoList();
            Assert.IsTrue(list.ClassListContains("unity-hidden"));

            list.items = SampleItems();
            Assert.IsFalse(list.ClassListContains("unity-hidden"));

            list.items = null;
            Assert.IsTrue(list.ClassListContains("unity-hidden"));
        }

        /// <summary>
        /// The setter copies, so a caller mutating its own list afterwards cannot desynchronise what is
        /// on screen.
        /// </summary>
        [Test]
        public void Items_AreCopiedNotAliased()
        {
            var source = new List<ChatTodoItem> { new ChatTodoItem("One") };
            var list = new ChatTodoList { items = source };
            source.Add(new ChatTodoItem("Two"));

            Assert.AreEqual(1, list.itemCount);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The status glyph slot is sized off the icon control-sizing alias, never a raw sizing step.
        /// The two resolve to the same pixels today, so only reading the source defends the binding.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void GlyphSlot_BindsToTheControlSizingIconAlias()
        {
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"\.appui-chat-todo-list__item-status");
            StringAssert.Contains("width: var(--appui-alias-control-sizing-icon-sm)", body);
            StringAssert.Contains("height: var(--appui-alias-control-sizing-icon-sm)", body);
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(body, @"(width|height):\s*var\(--appui-sizing-\d"),
                "The glyph slot must not size itself off a raw sizing step.");
        }

        [UnityTest]
        public IEnumerator Header_TogglesOpenWhenActivated()
        {
            var list = new ChatTodoList { items = SampleItems() };
            yield return ChatTestUtils.Mount(m_TestUI, list);

            Assert.IsTrue(list.open);
            list.clickable.InvokePressed(null);
            Assert.IsFalse(list.open, "Pressing the header must collapse the checklist.");
        }

        [Test]
        public void Collapsible_IsOnByDefault()
        {
            Assert.IsTrue(new ChatTodoList().collapsible);
        }

        /// <summary>
        /// A list that owns its surface has nothing to collapse into, so turning the header off has
        /// to force the body open and stop the press from closing it again.
        /// </summary>
        [Test]
        public void NotCollapsible_ForcesTheBodyOpenAndStopsRespondingToThePress()
        {
            var list = new ChatTodoList { items = SampleItems(), open = false };
            Assert.IsFalse(list.open);

            list.collapsible = false;

            Assert.IsTrue(list.open, "Turning off collapsing must open the body, not strand it closed.");
            var caret = list.Q(className: ChatTodoList.caretUssClassName);
            Assert.IsTrue(caret.ClassListContains(Styles.hiddenUssClassName),
                "With nothing to collapse, the caret is a lie.");

            list.clickable.InvokePressed(null);
            Assert.IsTrue(list.open, "A non-collapsible header must not close on a press.");
        }

        /// <summary>
        /// The registry shows each action only when its handler is passed. C# cannot omit a prop,
        /// so the flags are explicit and must both default to off.
        /// </summary>
        [Test]
        public void HeaderActions_AreHiddenUntilAskedFor()
        {
            var list = new ChatTodoList();
            var maximize = list.Q(className: ChatTodoList.maximizeUssClassName);
            var close = list.Q(className: ChatTodoList.closeUssClassName);

            Assert.IsFalse(list.showMaximize);
            Assert.IsFalse(list.showClose);
            Assert.IsTrue(maximize.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(close.ClassListContains(Styles.hiddenUssClassName));

            list.showMaximize = true;
            list.showClose = true;
            Assert.IsFalse(maximize.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(close.ClassListContains(Styles.hiddenUssClassName));
        }

        /// <summary>
        /// The actions are siblings of the caret, not children of it, so pressing one must not also
        /// toggle the list underneath.
        /// </summary>
        [Test]
        public void HeaderActions_ReportTheirOwnIntentWithoutTogglingTheList()
        {
            var list = new ChatTodoList { items = SampleItems(), showMaximize = true, showClose = true };
            var maximized = 0;
            var closed = 0;
            list.maximizeRequested += () => maximized++;
            list.closeRequested += () => closed++;

            var wasOpen = list.open;
            list.Q<IconButton>(className: ChatTodoList.maximizeUssClassName).clickable.InvokePressed(null);
            list.Q<IconButton>(className: ChatTodoList.closeUssClassName).clickable.InvokePressed(null);

            Assert.AreEqual(1, maximized);
            Assert.AreEqual(1, closed);
            Assert.AreEqual(wasOpen, list.open,
                "An action must not also toggle the list it sits on.");
        }

        [Test]
        public void Size_DefaultsToMdAndSwapsTheVariantClass()
        {
            var list = new ChatTodoList();
            Assert.AreEqual(ChatTodoListSize.Md, list.size);
            Assert.IsTrue(list.ClassListContains(ChatTodoList.GetSizeUssClassName(ChatTodoListSize.Md)));

            list.size = ChatTodoListSize.Sm;
            Assert.IsTrue(list.ClassListContains(ChatTodoList.GetSizeUssClassName(ChatTodoListSize.Sm)));
            Assert.IsFalse(list.ClassListContains(ChatTodoList.GetSizeUssClassName(ChatTodoListSize.Md)),
                "The two size variants must not both be applied.");
        }

        [Test]
        public void CompletedTone_DefaultsToPositive_AndSwapsTheToneClass()
        {
            var list = new ChatTodoList();
            Assert.AreEqual(ChatTodoCompletedTone.Positive, list.completedTone);
            Assert.IsTrue(list.ClassListContains("appui-chat-todo-list--completed-tone-positive"));

            list.completedTone = ChatTodoCompletedTone.Accent;
            Assert.IsTrue(list.ClassListContains("appui-chat-todo-list--completed-tone-accent"));
            Assert.IsFalse(list.ClassListContains("appui-chat-todo-list--completed-tone-positive"),
                "The two tones must not both be applied.");
        }

        /// <summary>
        /// The registry's <c>COMPLETED_TONE</c> map: positive reads as "this passed", accent as progress
        /// for a host whose other progress is drawn in the accent.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_CompletedTones_ReadTheRegistrysTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            const string check = @" \.appui-chat-todo-list__item--status-completed > \.appui-chat-todo-list__item-status > \.appui-icon";
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-semantic-positive)",
                ChatTestUtils.RuleBody(uss, @"^\.appui-chat-todo-list--completed-tone-positive" + check));
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-actions-accent-background-color-default)",
                ChatTestUtils.RuleBody(uss, @"^\.appui-chat-todo-list--completed-tone-accent" + check));
        }

        /// <summary>
        /// Resolved, not just written: the two tones paint the completed check differently.
        /// </summary>
        [UnityTest]
        public IEnumerator CompletedTone_ChangesTheResolvedCheckColour()
        {
            var positive = new ChatTodoList { items = SampleItems() };
            var accent = new ChatTodoList { items = SampleItems(), completedTone = ChatTodoCompletedTone.Accent };
            var column = new VisualElement();
            column.Add(positive);
            column.Add(accent);
            yield return ChatTestUtils.Mount(m_TestUI, column);

            Icon CheckOf(ChatTodoList list) =>
                list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.Completed)).Q<Icon>();

            Assert.AreNotEqual(CheckOf(positive).tintColor, CheckOf(accent).tintColor,
                "The accent tone must paint the completed check in another colour than the positive one.");
        }

        static ChatTodoItem[] LongPlan(int current)
        {
            var items = new ChatTodoItem[30];
            for (var i = 0; i < items.Length; i++)
            {
                var status = i < current ? ChatTodoStatus.Completed
                    : i == current ? ChatTodoStatus.InProgress
                    : ChatTodoStatus.Pending;
                items[i] = new ChatTodoItem("Step " + i, status);
            }
            return items;
        }

        static bool IsInView(ScrollView scroll, VisualElement row) =>
            row.worldBound.yMin >= scroll.contentViewport.worldBound.yMin - 0.5f &&
            row.worldBound.yMax <= scroll.contentViewport.worldBound.yMax + 0.5f;

        /// <summary>
        /// The registry keeps the in-progress item in view as it moves, on the list alone. A list long
        /// enough to scroll would otherwise leave the step being worked on below the fold.
        /// </summary>
        [UnityTest]
        public IEnumerator InProgressItem_IsScrolledIntoViewAsItMoves()
        {
            var list = new ChatTodoList { items = LongPlan(0) };
            yield return ChatTestUtils.Mount(m_TestUI, list);

            var scroll = list.Q<ScrollView>(ChatTodoList.itemsUssClassName);
            Assert.IsNotNull(scroll, "The items must sit in a ScrollView to scroll at all.");
            // The registry caps the body at max-h-80; the test caps it well below the list's height.
            scroll.style.maxHeight = 120f;
            for (var i = 0; i < 4; i++)
                yield return null;
            Assert.AreEqual(0f, scroll.scrollOffset.y, 0.5f, "The first step is already in view; nothing to scroll.");

            list.items = LongPlan(25);
            for (var i = 0; i < 4; i++)
                yield return null;
            var current = list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.InProgress));
            Assert.Greater(scroll.scrollOffset.y, 0f, "A step below the fold must be scrolled to.");
            Assert.IsTrue(IsInView(scroll, current), "The in-progress step must be in view.");

            list.items = LongPlan(2);
            for (var i = 0; i < 4; i++)
                yield return null;
            current = list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.InProgress));
            Assert.IsTrue(IsInView(scroll, current), "A step above the fold must be scrolled back to.");
        }

        /// <summary>
        /// A plan that grows from a few steps to a long one changes the scroll range in the same layout
        /// pass that places the current row. Scrolling before the range updates would clamp to the old,
        /// unscrollable range and leave the step out of view.
        /// </summary>
        [UnityTest]
        public IEnumerator InProgressItem_IsScrolledIntoViewWhenThePlanGrows()
        {
            var list = new ChatTodoList
            {
                items = new[] { new ChatTodoItem("Plan", ChatTodoStatus.InProgress), new ChatTodoItem("Do") },
            };
            yield return ChatTestUtils.Mount(m_TestUI, list);
            var scroll = list.Q<ScrollView>(ChatTodoList.itemsUssClassName);
            Assert.IsNotNull(scroll, "The items must sit in a ScrollView to scroll at all.");
            scroll.style.maxHeight = 120f;
            for (var i = 0; i < 2; i++)
                yield return null;

            list.items = LongPlan(25);
            for (var i = 0; i < 4; i++)
                yield return null;

            var current = list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.InProgress));
            Assert.IsTrue(IsInView(scroll, current), "The current step of a plan that grew must be in view.");
        }

        /// <summary>
        /// The registry mounts the list again on expand, which runs the scroll again. A list collapsed
        /// while the plan moved on has to bring the current step into view when it opens.
        /// </summary>
        [UnityTest]
        public IEnumerator InProgressItem_IsScrolledIntoViewWhenTheListOpens()
        {
            var list = new ChatTodoList { items = LongPlan(0), open = false };
            yield return ChatTestUtils.Mount(m_TestUI, list);
            var scroll = list.Q<ScrollView>(ChatTodoList.itemsUssClassName);
            Assert.IsNotNull(scroll, "The items must sit in a ScrollView to scroll at all.");
            scroll.style.maxHeight = 120f;

            list.items = LongPlan(25);
            for (var i = 0; i < 2; i++)
                yield return null;
            list.open = true;
            for (var i = 0; i < 4; i++)
                yield return null;

            var current = list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.InProgress));
            Assert.IsTrue(IsInView(scroll, current), "Opening the list must bring the current step into view.");
        }

        /// <summary>
        /// The same remount, with no new list: a reader who scrolled away from the current step, then
        /// folded the list and opened it again, finds the current step in view, as on the web.
        /// </summary>
        [UnityTest]
        public IEnumerator InProgressItem_IsScrolledBackIntoViewWhenTheListReopens()
        {
            var list = new ChatTodoList { items = LongPlan(0) };
            yield return ChatTestUtils.Mount(m_TestUI, list);
            var scroll = list.Q<ScrollView>(ChatTodoList.itemsUssClassName);
            Assert.IsNotNull(scroll, "The items must sit in a ScrollView to scroll at all.");
            scroll.style.maxHeight = 120f;
            list.items = LongPlan(25);
            for (var i = 0; i < 4; i++)
                yield return null;

            scroll.scrollOffset = UnityEngine.Vector2.zero;
            for (var i = 0; i < 2; i++)
                yield return null;
            var current = list.Q(className: ChatTodoList.GetStatusUssClassName(ChatTodoStatus.InProgress));
            Assert.IsFalse(IsInView(scroll, current), "The reader scrolled the current step out of view.");

            list.open = false;
            for (var i = 0; i < 2; i++)
                yield return null;
            list.open = true;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.IsTrue(IsInView(scroll, current), "Reopening the list must bring the current step back into view.");
        }

    }
}
