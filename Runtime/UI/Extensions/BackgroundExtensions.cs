using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Extension methods for <see cref="Background"/> objects.
    /// </summary>
    public static class BackgroundExtensions
    {

        /// <summary>
        /// Creates a new <see cref="Background"/> from a Unity image asset.
        /// </summary>
        /// <param name="obj"> The image asset to create the background from. </param>
        /// <returns> The created background. </returns>
        /// <remarks>
        /// The image asset can be a <see cref="Texture2D"/>, <see cref="Sprite"/>, <see cref="RenderTexture"/> or <see cref="VectorImage"/>.
        /// </remarks>
        public static Background FromObject(Object obj)
        {
            return obj switch
            {
                Texture2D tex => Background.FromTexture2D(tex),
                Sprite sprite => Background.FromSprite(sprite),
                RenderTexture rt => Background.FromRenderTexture(rt),
                VectorImage vi => Background.FromVectorImage(vi),
                _ => new Background()
            };
        }
    }
}
