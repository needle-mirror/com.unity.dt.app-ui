using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChatAssetRow))]
    class ChatAssetRowTests : VisualElementTests<ChatAssetRow>
    {
        const string k_Uss = "ChatAssetRow.uss";

        protected override string mainUssClassName => ChatAssetRow.ussClassName;

        // Handed over to ChatAssetRowSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Minimal", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                });
                yield return new Story("Full", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                    packLabel = "Sci-Fi Props Vol. 2",
                    creatorLabel = "Nebula Studios",
                    priceLabel = "$12",
                    details = true,
                    state = AssetRowState.Added,
                    statusText = "Added to the scene",
                });
                yield return new Story("Failed", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                    state = AssetRowState.Failed,
                    statusText = "Download failed",
                    statusTone = AssetRowStatusTone.Destructive,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ChatAssetRow />",
            @"<appui:ChatAssetRow title=""Crate"" source-label=""Asset Store"" state=""Added"" />",
            @"<appui:ChatAssetRow title=""Crate"" source-label=""Asset Store"" state=""Failed"" status-tone=""Destructive"" status-text=""nope"" />",
            @"<appui:ChatAssetRow title=""Crate"" source-label=""Asset Store"" size-label=""≈ 48 MB"" description=""Made from a photo"" tags=""FBX,4K"" />",
        };

        [Test]
        public void SizeLabelAndDescription_ShowWhenSetAndHideWhenEmpty()
        {
            var row = new ChatAssetRow { title = "Crate", sourceLabel = "Asset Store" };
            var size = row.Q(ChatAssetRow.sizeUssClassName);
            var description = row.Q(ChatAssetRow.descriptionUssClassName);
            Assert.IsNotNull(size, "The row must build a size line.");
            Assert.IsNotNull(description, "The row must build a description line.");
            Assert.IsTrue(size.ClassListContains(Styles.hiddenUssClassName), "An unset size leaves no empty line.");
            Assert.IsTrue(description.ClassListContains(Styles.hiddenUssClassName), "An unset description leaves no empty line.");

            row.sizeLabel = "≈ 48 MB";
            row.description = "Made from a photo of a crate.";
            Assert.IsFalse(size.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(description.ClassListContains(Styles.hiddenUssClassName));
            Assert.AreEqual("≈ 48 MB", ((TextElement)size).text);
            Assert.AreEqual("Made from a photo of a crate.", ((TextElement)description).text);

            row.sizeLabel = null;
            row.description = "";
            Assert.IsTrue(size.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(description.ClassListContains(Styles.hiddenUssClassName));
        }

        /// <summary>
        /// The registry draws each tag as a secondary Badge pill. UI Toolkit's <see cref="Badge"/> is a
        /// notification overlay, not a pill, so the row draws its own, one per tag.
        /// </summary>
        [Test]
        public void Tags_RenderOnePillEach_AndHideWhenEmpty()
        {
            var row = new ChatAssetRow { title = "Crate", sourceLabel = "Asset Store" };
            var tags = row.Q(ChatAssetRow.tagsUssClassName);
            Assert.IsNotNull(tags, "The row must build a tag strip.");
            Assert.IsTrue(tags.ClassListContains(Styles.hiddenUssClassName), "No tags, no strip.");

            var source = new List<string> { "FBX", "4K", "Rigged" };
            row.tags = source;
            Assert.IsFalse(tags.ClassListContains(Styles.hiddenUssClassName));
            var pills = tags.Query<TextElement>(className: ChatAssetRow.tagUssClassName).ToList();
            CollectionAssert.AreEqual(new[] { "FBX", "4K", "Rigged" }, pills.ConvertAll(p => p.text));

            source.Add("Late");
            Assert.AreEqual(3, tags.Query(className: ChatAssetRow.tagUssClassName).ToList().Count,
                "The row copies the tags it is given, so a caller mutating its own list does not desync it.");

            row.tags = new List<string>();
            Assert.IsTrue(tags.ClassListContains(Styles.hiddenUssClassName));
            Assert.AreEqual(0, tags.childCount);
        }

        [Test]
        public void Extra_IsTheLastLine_AndGoesAwayWhenCleared()
        {
            var row = new ChatAssetRow { title = "Crate", sourceLabel = "Asset Store", statusText = "Added" };
            var extra = new Unity.AppUI.UI.Button { title = "Edit" };
            row.extra = extra;

            var identity = row.Q(ChatAssetRow.identityUssClassName);
            Assert.AreSame(identity, extra.hierarchy.parent, "The extra slot sits in the identity block.");
            Assert.AreSame(extra, identity.hierarchy[identity.hierarchy.childCount - 1],
                "The extra slot is rendered under the row's facts, after the status line.");
            Assert.IsTrue(extra.ClassListContains(ChatAssetRow.extraUssClassName));

            row.extra = null;
            Assert.IsNull(extra.hierarchy.parent, "Clearing the slot removes the element.");
            Assert.IsFalse(extra.ClassListContains(ChatAssetRow.extraUssClassName),
                "A removed element must not keep the slot's class.");
        }

        /// <summary>
        /// The registry's order: title, source, creator, description, tags, price, size, status, then
        /// the consumer's extra.
        /// </summary>
        [Test]
        public void Lines_FollowTheRegistryOrder()
        {
            var row = new ChatAssetRow();
            var identity = row.Q(ChatAssetRow.identityUssClassName);
            var expected = new[]
            {
                ChatAssetRow.titleUssClassName,
                ChatAssetRow.sourceUssClassName,
                ChatAssetRow.creatorUssClassName,
                ChatAssetRow.descriptionUssClassName,
                ChatAssetRow.tagsUssClassName,
                ChatAssetRow.priceUssClassName,
                ChatAssetRow.sizeUssClassName,
                ChatAssetRow.statusUssClassName,
            };
            var actual = new List<string>();
            for (var i = 0; i < identity.hierarchy.childCount; i++)
                actual.Add(identity.hierarchy[i].name);
            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        /// The registry clamps the description to two lines so a long one never pushes the rows below
        /// out of view. USS has neither line-clamp nor line-height, so the row measures a line of its
        /// own text and caps the element at two of them.
        /// </summary>
        [UnityTest]
        public IEnumerator Description_ClampsToTwoLines()
        {
            var shortRow = new ChatAssetRow { title = "Crate", sourceLabel = "Asset Store", description = "One line." };
            var longRow = new ChatAssetRow
            {
                title = "Crate",
                sourceLabel = "Asset Store",
                description = string.Join(" ", System.Linq.Enumerable.Repeat("A long description that wraps.", 20)),
            };
            var column = new VisualElement { style = { width = 240f } };
            column.Add(shortRow);
            column.Add(longRow);
            yield return ChatTestUtils.Mount(m_TestUI, column);

            var oneLine = shortRow.Q(ChatAssetRow.descriptionUssClassName).layout.height;
            var clamped = longRow.Q(ChatAssetRow.descriptionUssClassName).layout.height;
            Assert.Greater(oneLine, 0f, "The one-line description did not lay out.");
            Assert.Greater(clamped, oneLine * 1.5f, "A long description still gets its second line.");
            Assert.LessOrEqual(clamped, oneLine * 2f + 1f, "A long description must stop at two lines.");

            // A density change moves the font size without moving the clamped box; the cap must follow.
            column.GetFirstAncestorOfType<Panel>().density = "compact";
            for (var i = 0; i < 4; i++)
                yield return null;

            var compactLine = shortRow.Q(ChatAssetRow.descriptionUssClassName).layout.height;
            var compactClamped = longRow.Q(ChatAssetRow.descriptionUssClassName).layout.height;
            Assert.Less(compactLine, oneLine, "Compact density must shrink the description's line for this check to mean anything.");
            Assert.LessOrEqual(compactClamped, compactLine * 2f + 1f, "After a density change the clamp must still be two lines.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_NewLines_ReadTheRegistrysTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("color: var(--appui-alias-text-tertiary)",
                ChatTestUtils.RuleBody(uss, @"^\.appui-chat-asset-row__size"), "The size reads the tertiary text role.");
            StringAssert.Contains("color: var(--appui-alias-text-secondary)",
                ChatTestUtils.RuleBody(uss, @"^\.appui-chat-asset-row__description"), "The description reads secondary.");

            var tag = ChatTestUtils.RuleBody(uss, @"^\.appui-chat-asset-row__tag");
            StringAssert.Contains("background-color: var(--appui-alias-control-background-color-default)", tag);
            StringAssert.Contains("color: var(--appui-alias-control-foreground-color-default)", tag);
            StringAssert.Contains("border-color: var(--appui-alias-control-border-color-default)", tag);

            StringAssert.Contains("flex-wrap: wrap", ChatTestUtils.RuleBody(uss, @"^\.appui-chat-asset-row__tags"),
                "Tags wrap onto a new line rather than overflowing the row.");
        }

        [Test]
        public void Defaults_AreThePendingMutedRow()
        {
            var row = new ChatAssetRow();
            Assert.AreEqual(AssetRowState.Pending, row.state);
            Assert.AreEqual(AssetRowStatusTone.Muted, row.statusTone);
            Assert.IsFalse(row.details, "The details link is opt-in.");
        }

        /// <summary>
        /// The generator lowercases the enum member, so the USS has to define exactly the four names
        /// it emits. A mismatch styles nothing at all, silently.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void StateAndToneClasses_MatchWhatTheGeneratorEmits()
        {
            var row = new ChatAssetRow { state = AssetRowState.Skipped, statusTone = AssetRowStatusTone.Destructive };
            Assert.IsTrue(row.ClassListContains("appui-chat-asset-row--state-skipped"));

            var status = row.Q(ChatAssetRow.statusUssClassName);
            Assert.IsTrue(status.ClassListContains("appui-chat-asset-row__status--destructive"));

            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains(".appui-chat-asset-row__status--muted", uss);
            StringAssert.Contains(".appui-chat-asset-row__status--destructive", uss);
        }

        [Test]
        public void ChangingStateOrTone_RemovesTheStaleClass()
        {
            var row = new ChatAssetRow { state = AssetRowState.Added };
            row.state = AssetRowState.Failed;
            Assert.IsFalse(row.ClassListContains("appui-chat-asset-row--state-added"));
            Assert.IsTrue(row.ClassListContains("appui-chat-asset-row--state-failed"));

            var status = row.Q(ChatAssetRow.statusUssClassName);
            row.statusTone = AssetRowStatusTone.Destructive;
            Assert.IsFalse(status.ClassListContains("appui-chat-asset-row__status--muted"));
            row.statusTone = AssetRowStatusTone.Muted;
            Assert.IsFalse(status.ClassListContains("appui-chat-asset-row__status--destructive"));
        }

        /// <summary>
        /// Every optional line hides itself when it has nothing to say, so an asset with no pack, no
        /// creator and no price does not leave three empty rows behind.
        /// </summary>
        [Test]
        public void OptionalLines_HideThemselvesWhenEmpty()
        {
            var row = new ChatAssetRow { title = "Crate", sourceLabel = "Asset Store" };

            foreach (var name in new[]
            {
                ChatAssetRow.packLabelUssClassName,
                ChatAssetRow.creatorUssClassName,
                ChatAssetRow.priceUssClassName,
                ChatAssetRow.statusUssClassName,
            })
            {
                Assert.IsTrue(row.Q(name).ClassListContains("unity-hidden"), $"{name} should be hidden when empty.");
            }

            Assert.IsFalse(row.Q(ChatAssetRow.sourceLabelUssClassName).ClassListContains("unity-hidden"),
                "The source is always shown.");
        }

        /// <summary>
        /// The separator only exists to join the source to the pack, so it must disappear with it —
        /// otherwise a row with no pack reads "Asset Store ·".
        /// </summary>
        [Test]
        public void Separator_DisappearsWithThePackLabel()
        {
            var row = new ChatAssetRow { sourceLabel = "Asset Store" };
            var separator = row.Q(ChatAssetRow.separatorUssClassName);
            Assert.IsTrue(separator.ClassListContains("unity-hidden"));

            row.packLabel = "Sci-Fi Props Vol. 2";
            Assert.IsFalse(separator.ClassListContains("unity-hidden"));

            row.packLabel = null;
            Assert.IsTrue(separator.ClassListContains("unity-hidden"));
        }

        [Test]
        public void NoThumbnail_ShowsThePlaceholderGlyph()
        {
            var row = new ChatAssetRow();
            var placeholder = row.Q(ChatAssetRow.thumbnailPlaceholderUssClassName);
            Assert.IsFalse(placeholder.ClassListContains("unity-hidden"),
                "An unset thumbnail shows the cube placeholder rather than an empty box.");
        }

        [Test]
        public void DetailsLink_RaisesItsEventOnlyWhenOffered()
        {
            var row = new ChatAssetRow { details = true };
            var seen = 0;
            row.detailsRequested += () => seen++;

            row.detailsButton.clickable.InvokePressed(null);
            Assert.AreEqual(1, seen);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The tone-to-token mapping lives in this file and nowhere else, which is the whole reason
        /// the tone is an enum rather than a class name a caller passes in.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void StatusTones_MapToTheDisabledAndDestructiveTokens()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);
            StringAssert.Contains("color: var(--appui-alias-text-disabled)",
                ChatTestUtils.RuleBody(uss, @"\.appui-chat-asset-row__status--muted"));
            StringAssert.Contains("color: var(--appui-alias-semantic-destructive)",
                ChatTestUtils.RuleBody(uss, @"\.appui-chat-asset-row__status--destructive"));
        }

        [Test]
        public void FallbackIcon_DefaultsToTheAssetCube()
        {
            Assert.AreEqual("cube", new ChatAssetRow().fallbackIcon);
        }

        /// <summary>
        /// A consumer that knows what kind of thing the row describes passes its own symbol, so an
        /// item with no picture still reads as what it is.
        /// </summary>
        [Test]
        public void FallbackIcon_IsReplaceableAndFallsBackWhenCleared()
        {
            var row = new ChatAssetRow { fallbackIcon = "music-note" };
            Assert.AreEqual("music-note", row.fallbackIcon);

            row.fallbackIcon = null;
            Assert.AreEqual("cube", row.fallbackIcon,
                "Clearing the symbol must restore the generic asset cube, never leave the row blank.");
        }

        /// <summary>
        /// A row whose content is not an image supplies its own medium. It has to replace the
        /// thumbnail rather than sit beside it, otherwise the row shows both.
        /// </summary>
        [Test]
        public void Media_ReplacesTheThumbnailAndRestoresItWhenCleared()
        {
            var row = new ChatAssetRow();
            var thumbnail = row.Q(className: ChatAssetRow.thumbnailUssClassName);
            Assert.IsFalse(thumbnail.ClassListContains(Styles.hiddenUssClassName));

            var medium = new VisualElement();
            row.media = medium;
            Assert.AreSame(row, medium.hierarchy.parent);
            Assert.IsTrue(medium.ClassListContains(ChatAssetRow.mediaUssClassName));
            Assert.IsTrue(thumbnail.ClassListContains(Styles.hiddenUssClassName),
                "A supplied medium must replace the thumbnail, not sit beside it.");

            row.media = null;
            Assert.IsNull(medium.hierarchy.parent);
            Assert.IsFalse(thumbnail.ClassListContains(Styles.hiddenUssClassName));
        }

        /// <summary>
        /// A source that knows a creator's name but no page must still show the name, so the link
        /// is opt-in rather than implied by the label.
        /// </summary>
        [Test]
        public void CreatorLink_IsOffByDefaultAndReportsIntentWhenOn()
        {
            var row = new ChatAssetRow { creatorLabel = "Someone" };
            var creator = row.Q(className: ChatAssetRow.creatorUssClassName);

            Assert.IsFalse(row.creatorLink);
            Assert.IsFalse(creator.ClassListContains(ChatAssetRow.creatorLinkUssClassName),
                "A creator with no page must read as plain text.");

            var raised = 0;
            row.creatorRequested += () => raised++;
            row.creatorLink = true;

            Assert.IsTrue(creator.ClassListContains(ChatAssetRow.creatorLinkUssClassName));
            Assert.AreEqual(PickingMode.Position, creator.pickingMode,
                "A link that cannot be picked is not a link.");

            row.creatorPressable.InvokePressed(null);
            Assert.AreEqual(1, raised, "Activating the creator label must report the intent.");

            row.creatorLink = false;
            Assert.IsFalse(creator.ClassListContains(ChatAssetRow.creatorLinkUssClassName));
            Assert.AreEqual(PickingMode.Ignore, creator.pickingMode);
        }
    }
}
