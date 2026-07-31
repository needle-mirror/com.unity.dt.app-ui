using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the AvatarGroup documentation page.</summary>
    static class AvatarGroupDemos
    {
        [VisualDocDemo("avatargroup")]
        static VisualElement Basic()
        {
            var users = new List<string> { "MB", "JS", "AL", "TK", "RW" };

            var avatarGroup = new AvatarGroup { max = 4, spacing = AvatarGroupSpacing.M };
            avatarGroup.sourceItems = users;
            avatarGroup.bindItem = (avatar, index) =>
            {
                avatar.label = users[index];
                avatar.backgroundColor = new Color(0.30f, 0.44f, 0.93f);
                avatar.autoLabelColor = true;
            };

            return DemoUtils.Row(avatarGroup);
        }
    }
}
