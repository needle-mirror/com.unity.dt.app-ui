using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadReactionBar UI element. Displays a row of reactions with an add-reaction button.
    /// </summary>
    [UxmlElement]
    public partial class ThreadReactionBar : BaseVisualElement
    {
        /// <summary>
        /// The ThreadReactionBar main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-reaction-bar";

        /// <summary>
        /// The ThreadReactionBar add button styling class.
        /// </summary>
        public const string addButtonUssClassName = ussClassName + "__add-button";

        readonly ActionButton m_AddButton;

        IList<ReactionInfo> m_Reactions;

        Popover m_Popover;

        /// <summary>
        /// Event fired when a reaction is toggled. Parameters are (emoji, isAdding).
        /// </summary>
        public event Action<string, bool> reactionToggled;

        /// <summary>
        /// The reactions data source. Setting this rebuilds the reaction children.
        /// </summary>
        public IList<ReactionInfo> reactions
        {
            get => m_Reactions;
            set
            {
                m_Reactions = value;
                Refresh();
            }
        }

        /// <summary>
        /// The content container of the ThreadReactionBar.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreadReactionBar()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            m_AddButton = new ActionButton
            {
                name = addButtonUssClassName,
                quiet = true,
                icon = "smiley",
                size = Size.M
            };
            m_AddButton.AddToClassList(addButtonUssClassName);
            m_AddButton.clicked += OnAddButtonClicked;
            hierarchy.Add(m_AddButton);

            this.RegisterContextChangedCallback<EmojisContext>(OnEmojisContextChanged);
        }

        void OnEmojisContextChanged(ContextChangedEvent<EmojisContext> evt)
        {
            var isValid = evt.context is { IsValid: true };
            m_AddButton.EnableInClassList(Styles.hiddenUssClassName, !isValid);
        }

        void Refresh()
        {
            if (m_Reactions == null)
            {
                // Remove all ThreadReaction children
                for (var i = hierarchy.childCount - 1; i >= 0; i--)
                {
                    if (hierarchy[i] is ThreadReaction reaction)
                    {
                        reaction.clickable.clickedWithEventInfo -= OnReactionClicked;
                        hierarchy.RemoveAt(i);
                    }
                }
                return;
            }

            // Update or add reactions
            var insertIndex = 0;
            foreach (var reactionInfo in m_Reactions)
            {
                ThreadReaction reaction;

                // Try to find an existing ThreadReaction at this position
                if (insertIndex < hierarchy.childCount && hierarchy[insertIndex] is ThreadReaction existingReaction)
                {
                    reaction = existingReaction;
                }
                else
                {
                    // No existing element — insert a new one
                    reaction = new ThreadReaction();
                    reaction.clickable.clickedWithEventInfo += OnReactionClicked;
                    hierarchy.Insert(insertIndex, reaction);
                }

                reaction.emoji = reactionInfo.emoji;
                reaction.count = reactionInfo.count;
                reaction.isOwnReaction = reactionInfo.isOwnReaction;

                insertIndex++;
            }

            // Remove any excess ThreadReaction elements
            for (var i = hierarchy.childCount - 1; i >= insertIndex; i--)
            {
                if (hierarchy[i] is ThreadReaction excess)
                {
                    excess.clickable.clickedWithEventInfo -= OnReactionClicked;
                    hierarchy.RemoveAt(i);
                }
            }
        }

        void OnReactionClicked(EventBase e)
        {
            if (e.target is ThreadReaction reaction)
                reactionToggled?.Invoke(reaction.emoji, !reaction.isOwnReaction);
        }

        void OnAddButtonClicked()
        {
            var ctx = this.GetContext<EmojisContext>();
            if (ctx is not { IsValid: true })
                return;

            var picker = new ReactionPicker { emojiProvider = ctx.database };
            picker.emojiSelected += OnEmojiSelectedFromPicker;

            m_Popover = Popover.Build(m_AddButton, picker);
            m_Popover.SetPlacement(PopoverPlacement.Top);
            m_Popover.SetOffset(12);
            m_Popover.SetArrowVisible(false);
            m_Popover.Show();
        }

        void OnEmojiSelectedFromPicker(string emoji)
        {
            reactionToggled?.Invoke(emoji, true);
            m_Popover?.Dismiss();
            m_Popover = null;
        }

    }
}
