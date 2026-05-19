using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Editor
{
    class ThreadMessageStories : StoryBookPage
    {
        public override string displayName => "Thread Message";

        public ThreadMessageStories()
        {
            AddStory("Default", () => new ThreadMessage
            {
                authorName = "John Doe",
                authorInitials = "JD",
                message = "This is a sample message in the thread.",
                timestamp = "2 hours ago",
#if UNITY_2022_3_OR_NEWER
                authorAvatar = null,
#endif
                authorAvatarColor = Color.magenta,
                state = ThreadMessageState.Draft,
                makeActionMenuItems = null,
                reactions = new List<ReactionInfo>(),
                timestampTooltip = "Sent on June 1, 2024 at 3:45 PM",
                isLiked = false,
                isDisliked = false,
                likeCount = 0,
                dislikeCount = 0,
            });
        }
    }
}
