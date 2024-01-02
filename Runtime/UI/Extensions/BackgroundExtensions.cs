using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    public static class BackgroundExtensions
    {
#if !UNITY_2023_2_OR_NEWER
        public static Object GetSelectedImage(this Background bg)
        {
            if (bg == default)
                return (UnityEngine.Object) null;

            if ((UnityEngine.Object) bg.texture != (UnityEngine.Object) null)
                return (UnityEngine.Object) bg.texture;
            if ((UnityEngine.Object) bg.sprite != (UnityEngine.Object) null)
                return (UnityEngine.Object) bg.sprite;
            if ((UnityEngine.Object) bg.renderTexture != (UnityEngine.Object) null)
                return (UnityEngine.Object) bg.renderTexture;
            return (UnityEngine.Object) bg.vectorImage != (UnityEngine.Object) null ? (UnityEngine.Object) bg.vectorImage : (UnityEngine.Object) null;
        }
#endif

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
