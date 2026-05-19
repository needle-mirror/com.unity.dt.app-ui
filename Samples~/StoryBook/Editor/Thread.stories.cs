using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Avatar = Unity.AppUI.UI.Avatar;
using Thread = Unity.AppUI.UI.Thread;

namespace Unity.AppUI.Editor
{
    class SampleMentionProvider : IMentionProvider
    {
        static readonly MentionInfo[] k_Users =
        {
            new MentionInfo
            {
                id = "1234567890",
                title = "Alice",
                avatarColor = new Color(0.30f, 0.44f, 0.93f),
#if UNITY_2022_3_OR_NEWER
                avatarSrc = null,
#endif
                subtitle = "alice@company.com",
            },
            new MentionInfo
            {
                id = "9876543211011",
                title = "Bob",
                avatarColor = new Color(0.20f, 0.72f, 0.47f),
#if UNITY_2022_3_OR_NEWER
                avatarSrc = null,
#endif
                subtitle = "bob@company.com"
            },
            new MentionInfo { id = "1111111111", title = "Carol" },
            new MentionInfo { id = "2222222222", title = "Dave" },
            new MentionInfo { id = "3333333333", title = "Eve" },
            new MentionInfo { id = "4444444444", title = "Frank" },
            new MentionInfo { id = "5555555555", title = "Grace" },
            new MentionInfo { id = "6666666666", title = "Hank" },
        };

        // Matches :entityType[displayName]{#entityId}
        static readonly Regex k_MentionRegex = new Regex(@":(\w+)\[([^\]]+)\]\{#([^}]+)\}", RegexOptions.Compiled);

        public IList GetSuggestions(string query)
        {
            return k_Users.Where(u =>
                u.title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                (u.subtitle != null && u.subtitle.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
        }

        public string ConvertToRichText(string rawContent)
        {
            return k_MentionRegex.Replace(rawContent, match =>
            {
                var displayName = match.Groups[2].Value;
                return "<a href=\"https://google.com\"><b>@" + displayName + "</b></a>";
            });
        }

        public string GetDisplayName(object mention)
        {
            if (mention is MentionInfo info)
                return info.title;

            return mention.ToString();
        }

        public VisualElement MakeSuggestionItem()
        {
            var item = new ListViewItem();
            var avatar = new Avatar
            {
                size = Size.M,
                backgroundColor = Color.black,
                autoLabelColor = true,
                labelColor = Color.white,
                label = "A"
            };
            item.leadingContainer.Add(avatar);
            item.optionsButton.style.display = DisplayStyle.None;
            item.style.borderBottomLeftRadius = 0;
            item.style.borderBottomRightRadius = 0;
            item.style.borderTopLeftRadius = 0;
            item.style.borderTopRightRadius = 0;
            return item;
        }

        public void BindItem(VisualElement item, object data)
        {
            if (item is not ListViewItem listViewItem)
                return;

            var mention = (MentionInfo)data;
            listViewItem.userData = mention;
            listViewItem.title = mention.title;
            listViewItem.subtitle = mention.subtitle;
            var avatar = listViewItem.Q<Avatar>();
            if (mention.avatarSrc != null)
            {
                avatar.src = mention.avatarSrc;
                avatar.label = null;
            }
            else
            {
#if UNITY_2022_3_OR_NEWER
                avatar.src = null;
#endif
                avatar.label = mention.title.GetInitials();
                avatar.backgroundColor = mention.avatarColor;
            }
        }

        public string Encode(object data)
        {
            var mention = (MentionInfo)data;
            return ":user[" + mention.title + "]{#" + mention.id + "}";
        }
    }

    /// <summary>
    /// Represents a mentionable user or entity.
    /// </summary>
    public struct MentionInfo
    {
        /// <summary>
        /// The unique identifier of the mentioned entity.
        /// </summary>
        public string id;

        /// <summary>
        /// The display name of the mentioned entity.
        /// </summary>
        public string title;

        /// <summary>
        /// The avatar image source for the mentioned entity.
        /// </summary>
        public Background avatarSrc;

        /// <summary>
        /// The subtitle or additional info for the mentioned entity (e.g., email or role).
        /// </summary>
        public string subtitle;

        /// <summary>
        /// The avatar color to use if the avatar image fails to load, represented as a hex string (e.g., "#FF5733").
        /// </summary>
        public Color avatarColor;
    }

    enum MessageType
    {
        Normal,
        Resolve,
    }

    struct AttachmentData
    {
        public string title;
        public string subtitle;
        public string iconName;
    }

    struct MessageData
    {
        public MessageType type;
        public string authorName;
        public string timestamp;
        public string timestampTooltip;
        public string content;
        public int colorIndex;
        public int likeCount;
        public int dislikeCount;
        public bool isLiked;
        public bool isDisliked;
        public List<ReactionInfo> reactions;
        public List<AttachmentData> attachmentData;
        public Action<ThreadMessage, MenuBuilder> makeActionMenuItems;
    }

    public class ThreadPage : StoryBookPage
    {
        public override string displayName => "Thread";

        public override Type componentType => typeof(ThreadComponent);

        const int EditActionId = 1;

        const int DeleteActionId = 2;

        static readonly Color[] k_AvatarColors =
        {
            new Color(0.30f, 0.44f, 0.93f), // blue
            new Color(0.20f, 0.72f, 0.47f), // green
            new Color(0.91f, 0.40f, 0.33f), // red
            new Color(0.60f, 0.36f, 0.85f), // purple
            new Color(0.95f, 0.61f, 0.20f), // orange
        };

        struct ParticipantData
        {
            public string name;
            public int colorIndex;
        }

        static void SetParticipants(Thread thread, params string[] names)
        {
            var data = new List<ParticipantData>();
            for (var i = 0; i < names.Length; i++)
                data.Add(new ParticipantData { name = names[i], colorIndex = i });

            thread.participants = data;
            thread.bindParticipant = (avatar, index) =>
            {
                var p = data[index];
                avatar.backgroundColor = k_AvatarColors[p.colorIndex % k_AvatarColors.Length];
                avatar.autoLabelColor = true;
                avatar.label = p.name.GetInitials();
            };
        }

        static void SetupRepliesListView(Thread thread, List<MessageData> messages)
        {
            var listView = thread.repliesListView;
            listView.itemsSource = messages;
            listView.makeItem = () => new VisualElement();
            listView.bindItem = (element, index) =>
            {
                element.Clear();
                var data = messages[index];

                if (data.type == MessageType.Resolve)
                {
                    var resolveMsg = new ThreadResolveMessage
                    {
                        authorName = data.authorName,
                        timestamp = data.timestamp,
                    };
                    element.Add(resolveMsg);
                    return;
                }

                var msg = new ThreadMessage
                {
                    authorName = data.authorName,
                    timestamp = data.timestamp,
                    message = data.content,
                    authorInitials = data.authorName.GetInitials(),
                    authorAvatarColor = k_AvatarColors[data.colorIndex % k_AvatarColors.Length],
                    likeCount = data.likeCount,
                    dislikeCount = data.dislikeCount,
                    isLiked = data.isLiked,
                    isDisliked = data.isDisliked,
                    makeActionMenuItems = data.makeActionMenuItems,
                };

                if (!string.IsNullOrEmpty(data.timestampTooltip))
                    msg.timestampTooltip = data.timestampTooltip;

                if (data.reactions != null)
                    msg.reactions = data.reactions;

                if (data.attachmentData != null)
                {
                    foreach (var att in data.attachmentData)
                    {
                        var attachment = new Attachment
                        {
                            title = att.title,
                            subtitle = att.subtitle,
                        };
                        attachment.Add(new Icon { iconName = att.iconName });
                        var title = att.title;
                        attachment.deleteClicked += () => Debug.Log($"Delete attachment: {title}");
                        msg.attachments.Add(attachment);
                    }
                }

                msg.likeToggled += liked =>
                {
                    msg.likeCount += liked ? 1 : -1;
                    if (liked && msg.isDisliked)
                    {
                        msg.isDisliked = false;
                        msg.dislikeCount--;
                    }
                };

                msg.dislikeToggled += disliked =>
                {
                    msg.dislikeCount += disliked ? 1 : -1;
                    if (disliked && msg.isLiked)
                    {
                        msg.isLiked = false;
                        msg.likeCount--;
                    }
                };

                element.Add(msg);
            };

            listView.RefreshItems();
        }

        public ThreadPage()
        {
            m_Stories.Add(new StoryBookStory("Default", DefaultStory));
            m_Stories.Add(new StoryBookStory("Resolved", ResolvedStory));
            m_Stories.Add(new StoryBookStory("With Reactions", WithReactionsStory));
            m_Stories.Add(new StoryBookStory("Collapsed", CollapsedStory));
            m_Stories.Add(new StoryBookStory("No Replies", NoReply));
            m_Stories.Add(new StoryBookStory("With Attachments", WithAttachmentsStory));
            m_Stories.Add(new StoryBookStory("With Drop Zone", WithDropZoneStory));
        }

        static void ProvideContexts(VisualElement container)
        {
            var mentionProvider = new SampleMentionProvider();
            container.ProvideContext(new ThreadContext(
                enableReactions: true,
                enableLikes: true,
                enableDislikes: false,
                enableResolution: true,
                mentionProvider: mentionProvider));
            const string defaultEmojiDbGuid = "d3e7f2a1b4c5089612f3a8d7c6b5e401";
            var defaultEmojis = AssetDatabase.LoadAssetAtPath<EmojiDatabase>(AssetDatabase.GUIDToAssetPath(defaultEmojiDbGuid));
            container.ProvideContext(new EmojisContext(defaultEmojis));
        }

        static VisualElement DefaultStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = true,
                enableReactions = true,
                enableLikes = true,
                replyCount = 2,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Alice",
                    timestamp = "2 hours ago",
                    timestampTooltip = "March 8, 2026 10:00 AM",
                    content = "Has anyone looked into the rendering issue on the latest build?",
                    colorIndex = 0,
                    makeActionMenuItems = (message, builder) =>
                    {
                        if (message.authorName == "Alice")
                        {
                            builder.AddAction(EditActionId, "Edit", "pen", _ => StartEditing(message));
                            builder.AddAction(DeleteActionId, item =>
                            {
                                item.label = "Delete";
                                item.icon = "delete";
                                item.clickable.clicked += () => Debug.Log($"Delete action on message: {message.message}");
                            });
                        }
                    },
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Bob",
                    timestamp = "1 hour ago",
                    timestampTooltip = "March 8, 2026 11:00 AM",
                    content = "Yes, it seems related to the shader compilation step. :user[Carol]{#1234567890} can you confirm?",
                    colorIndex = 1,
                    likeCount = 3,
                    isLiked = true,
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Carol",
                    timestamp = "30 min ago",
                    timestampTooltip = "March 8, 2026 11:30 AM",
                    content = "I can confirm :user[Bob]{#9876543211011}. I have a fix ready for review.",
                    colorIndex = 2,
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Alice", "Bob", "Carol");

            thread.resolveToggled += resolved => Debug.Log($"Thread resolved: {resolved}");

            var composer = new ThreadComposer
            {
                textArea =
                {
                    submitOnEnter = true,
                    submitActionKeyModifier = true
                }
            };
            composer.submitClicked += msg => Debug.Log($"Message submitted: {msg}");

            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static void StartEditing(ThreadMessage message)
        {
            message.state = ThreadMessageState.Draft;
            var composer = new ThreadComposer { showAvatar = true, isEditing = true, value = message.message };
            composer.saveClicked += () => StopEditing(message, composer, true);
            composer.cancelClicked += () => StopEditing(message, composer, false);
            message.Add(composer);
            message.schedule.Execute(composer.textArea.Focus).ExecuteLater(16);
        }

        static void StopEditing(ThreadMessage message, ThreadComposer composer, bool submit)
        {
            if (submit)
            {
                message.message = composer.value;
            }
            composer.RemoveFromHierarchy();
            message.state = ThreadMessageState.Default;
        }

        static VisualElement ResolvedStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = true,
                isResolved = true,
                enableReactions = false,
                enableLikes = true,
                replyCount = 2,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Dave",
                    timestamp = "1 day ago",
                    content = "We need to update the CI pipeline configuration.",
                    colorIndex = 3,
                    likeCount = 5,
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Eve",
                    timestamp = "20 hours ago",
                    content = "Done. Pushed the updated config.",
                    colorIndex = 4,
                    likeCount = 2,
                },
                new MessageData
                {
                    type = MessageType.Resolve,
                    authorName = "Dave",
                    timestamp = "18 hours ago",
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Dave", "Eve");

            var composer = new ThreadComposer();
            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static VisualElement WithReactionsStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = false,
                enableReactions = true,
                enableLikes = true,
                replyCount = 1,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Frank",
                    timestamp = "3 hours ago",
                    content = "The new UI components are looking great!",
                    colorIndex = 0,
                    likeCount = 7,
                    isLiked = true,
                    reactions = new List<ReactionInfo>
                    {
                        new ReactionInfo { emoji = "\U0001F525", count = 4, isOwnReaction = true },
                        new ReactionInfo { emoji = "\U0001F680", count = 2, isOwnReaction = false },
                        new ReactionInfo { emoji = "\u2764\uFE0F", count = 1, isOwnReaction = false },
                    },
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Grace",
                    timestamp = "2 hours ago",
                    content = "Agreed! The thread component is especially nice.",
                    colorIndex = 1,
                    likeCount = 2,
                    reactions = new List<ReactionInfo>
                    {
                        new ReactionInfo { emoji = "\U0001F44D", count = 3, isOwnReaction = true },
                    },
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Frank", "Grace");

            var composer = new ThreadComposer();
            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static VisualElement CollapsedStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = true,
                enableReactions = true,
                enableLikes = true,
                replyCount = 5,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Hank",
                    timestamp = "1 week ago",
                    content = "Discussion about the new architecture proposal.",
                    colorIndex = 3,
                    likeCount = 12,
                },
            };

            for (var i = 0; i < 5; i++)
            {
                messages.Add(new MessageData
                {
                    type = MessageType.Normal,
                    authorName = $"User {i + 1}",
                    timestamp = $"{5 - i} days ago",
                    content = $"Reply number {i + 1} to the architecture discussion.",
                    colorIndex = i,
                });
            }

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Hank", "User 1", "User 2", "User 3", "User 4", "User 5");

            var composer = new ThreadComposer();
            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static VisualElement WithAttachmentsStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = false,
                enableReactions = false,
                enableLikes = true,
                replyCount = 1,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Alice",
                    timestamp = "10 min ago",
                    content = "Here are the design mockups for the new dashboard.",
                    colorIndex = 0,
                    attachmentData = new List<AttachmentData>
                    {
                        new AttachmentData { title = "dashboard-v2.fig", subtitle = "4.2 MB", iconName = "file" },
                        new AttachmentData { title = "screenshot.png", subtitle = "1.1 MB", iconName = "image" },
                    },
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Bob",
                    timestamp = "5 min ago",
                    content = "Looks great! I've attached my feedback notes.",
                    colorIndex = 1,
                    attachmentData = new List<AttachmentData>
                    {
                        new AttachmentData { title = "feedback-notes.pdf", subtitle = "340 KB", iconName = "file-text" },
                    },
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Alice", "Bob");

            var composer = new ThreadComposer();
            var composerAttachment = new Attachment
            {
                title = "draft-spec.docx",
                subtitle = "780 KB",
            };
            composerAttachment.Add(new Icon { iconName = "file-text" });
            composerAttachment.deleteClicked += () => Debug.Log("Delete composer attachment: draft-spec.docx");
            composer.attachments.Add(composerAttachment);

            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static VisualElement WithDropZoneStory()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = false,
                enableReactions = false,
                enableLikes = true,
                replyCount = 1,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Alice",
                    timestamp = "5 min ago",
                    content = "Can you attach the design files?",
                    colorIndex = 0,
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Bob",
                    timestamp = "2 min ago",
                    content = "Sure, let me drag them in.",
                    colorIndex = 1,
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Alice", "Bob");

            var composer = new ThreadComposer();

            // Configure drop zone to accept strings (file paths from Unity Project window)
            var dropController = composer.dropZone.controller;
            dropController.acceptDrag = draggedObjects => draggedObjects.Any(o => o is string);
            dropController.dropped += async droppedItems =>
            {
                foreach (var obj in droppedItems)
                {
                    if (obj is string path)
                    {
                        var attachment = new Attachment
                        {
                            title = System.IO.Path.GetFileName(path),
                            subtitle = "Dropped",
                        };
                        attachment.Add(await GetVisual(path));
                        attachment.deleteClicked += () =>
                        {
                            attachment.RemoveFromHierarchy();
                            composer.dropZone.visibleIndicator = false;
                        };
                        composer.attachments.Add(attachment);
                    }
                }
                composer.dropZone.visibleIndicator = false;
            };
            dropController.dragEnded += () => composer.dropZone.visibleIndicator = false;

#if UNITY_EDITOR
            container.RegisterCallback<DragUpdatedEvent>(_ => composer.dropZone.visibleIndicator = true);
            container.RegisterCallback<DragLeaveEvent>(_ => composer.dropZone.visibleIndicator = false);
            container.RegisterCallback<DragExitedEvent>(_ => composer.dropZone.visibleIndicator = false);
#endif
            composer.pasted += async evt =>
            {
                var png = await Platform.GetPasteboardDataAsync(PasteboardType.PNG);
                if (png is { Length: > 0 })
                {
                    evt.StopImmediatePropagation();
                    var texture = new Texture2D(2, 2);
                    texture.LoadImage(png);
                    var attachment = new Attachment
                    {
                        title = "Pasted Image",
                        subtitle = "From clipboard",
                    };
                    attachment.Add(new Image { image = texture });
                    attachment.deleteClicked += () =>
                    {
                        attachment.RemoveFromHierarchy();
                    };
                    composer.attachments.Add(attachment);
                }
            };

            composer.submitClicked += msg => Debug.Log($"Message submitted: {msg}");

            container.Add(thread);
            container.Add(composer);

            return container;
        }

        static async Task<VisualElement> GetVisual(string path)
        {
            if (IsSupportedImageFileFormat(path))
            {
                var texture = new Texture2D(2, 2);
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                texture.LoadImage(bytes);
                return new Image { image = texture, scaleMode = ScaleMode.ScaleToFit };
            }
            else
            {
                var icon = AssetDatabase.GetCachedIcon(path);
                if (icon)
                    return new Image { image = icon };
                else
                    return new Icon { iconName = "file" };
            }
        }

        static bool IsSupportedImageFileFormat(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif";
        }

        static VisualElement NoReply()
        {
            var container = new VisualElement { style = { maxWidth = 300 } };
            ProvideContexts(container);

            var thread = new Thread
            {
                enableResolution = true,
                enableReactions = true,
                enableLikes = true,
                replyCount = 0,
            };

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Hank",
                    timestamp = "1 week ago",
                    content = "Discussion about the new architecture proposal.",
                    colorIndex = 3,
                    likeCount = 12,
                },
            };

            SetupRepliesListView(thread, messages);
            SetParticipants(thread, "Hank");

            var composer = new ThreadComposer();
            container.Add(thread);
            container.Add(composer);

            return container;
        }
    }

    public class ThreadComponent : StoryBookComponent
    {
        public override Type uiElementType => typeof(Thread);

        public override void Setup(VisualElement element)
        {
            var thread = (Thread)element;
            thread.enableResolution = true;
            thread.enableReactions = true;
            thread.enableLikes = true;
            thread.replyCount = 1;

            var messages = new List<MessageData>
            {
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Author",
                    timestamp = "just now",
                    content = "This is a sample thread topic message.",
                    colorIndex = 0,
                },
                new MessageData
                {
                    type = MessageType.Normal,
                    authorName = "Replier",
                    timestamp = "just now",
                    content = "This is a sample reply.",
                    colorIndex = 1,
                },
            };

            var avatarColors = new[] { new Color(0.30f, 0.44f, 0.93f), new Color(0.20f, 0.72f, 0.47f) };

            var listView = thread.repliesListView;
            listView.itemsSource = messages;
            listView.makeItem = () => new VisualElement();
            listView.bindItem = (ve, index) =>
            {
                ve.Clear();
                var data = messages[index];
                var msg = new ThreadMessage
                {
                    authorName = data.authorName,
                    timestamp = data.timestamp,
                    message = data.content,
                    authorInitials = data.authorName.GetInitials(),
                    authorAvatarColor = avatarColors[data.colorIndex % avatarColors.Length],
                };
                ve.Add(msg);
            };
            listView.RefreshItems();

            var participantNames = new[] { "Author", "Replier" };
            thread.participants = participantNames;
            thread.bindParticipant = (avatar, index) =>
            {
                avatar.backgroundColor = avatarColors[index];
                avatar.autoLabelColor = true;
                var name = participantNames[index];
                var initials = name.Length >= 2
                    ? $"{char.ToUpper(name[0])}{char.ToUpper(name[1])}"
                    : $"{char.ToUpper(name[0])}";
                avatar.label = initials;
            };
        }

        public ThreadComponent()
        {
            m_Properties.Add(new StoryBookBooleanProperty(
                nameof(Thread.enableResolution),
                el => ((Thread)el).enableResolution,
                (el, val) => ((Thread)el).enableResolution = val));

            m_Properties.Add(new StoryBookBooleanProperty(
                nameof(Thread.isResolved),
                el => ((Thread)el).isResolved,
                (el, val) => ((Thread)el).isResolved = val));

            m_Properties.Add(new StoryBookBooleanProperty(
                nameof(Thread.enableReactions),
                el => ((Thread)el).enableReactions,
                (el, val) => ((Thread)el).enableReactions = val));

            m_Properties.Add(new StoryBookBooleanProperty(
                nameof(Thread.enableLikes),
                el => ((Thread)el).enableLikes,
                (el, val) => ((Thread)el).enableLikes = val));
        }
    }
}
