using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A visual element that indicates content is being loaded or processed.
    /// </summary>
    /// <remarks>
    /// The Preloader component provides visual feedback to users when content is loading or an operation is in
    /// progress. It consists of a circular progress animation with an optional centered logo, making it ideal for
    /// initial app loading screens or during resource-intensive operations.
    ///
    /// The component uses a circular progress indicator that rotates smoothly to indicate ongoing activity. The logo
    /// element can be customized through USS styling to match your application's branding.
    ///
    /// By default, the Preloader ignores user input events (PickingMode.Ignore) as it's designed to be a visual
    /// indicator only.
    ///
    /// The Preloader component consists of two main elements: a CircularProgress element that provides the rotating
    /// animation and a centered Image element that can display a logo through USS styling.
    /// </remarks>
    /// <example>
    /// <para>Basic usage of the Preloader component: Adding a basic preloader to a parent element.</para>
    /// <code lang="csharp">
    /// // Create a new Preloader
    /// var preloader = new Preloader();
    ///
    /// // Add it to a parent element
    /// parentElement.Add(preloader);
    /// </code>
    /// <para>Customizing the Preloader appearance using USS: Styling the preloader with custom size, colors, and logo.</para>
    /// <code lang="csharp">
    /// .appui-preloader {
    ///     width: 100px;
    ///     height: 100px;
    ///     background-color: rgba(0, 0, 0, 0.5);
    /// }
    ///
    /// .appui-preloader__circular-progress {
    ///     --progress-color: rgb(0, 122, 255);
    /// }
    ///
    /// .appui-preloader__logo {
    ///     --unity-image: url('project://path-to-your-logo.png');
    ///     width: 50%;
    ///     height: 50%;
    /// }
    /// </code>
    /// <para>Using the Preloader in a loading screen: Creating a full-screen loading screen with a centered preloader.</para>
    /// <code lang="csharp">
    /// public class LoadingScreen : VisualElement
    /// {
    ///     public LoadingScreen()
    ///     {
    ///         style.flexGrow = 1;
    ///         style.alignItems = Align.Center;
    ///         style.justifyContent = Justify.Center;
    ///
    ///         var preloader = new Preloader();
    ///         preloader.style.width = 120;
    ///         preloader.style.height = 120;
    ///
    ///         Add(preloader);
    ///     }
    /// }
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class Preloader : BaseVisualElement
    {
        /// <summary>
        /// The Preloader's USS class name.
        /// </summary>
        public const string ussClassName = "appui-preloader";

        /// <summary>
        /// The Preloader's circular progress USS class name.
        /// </summary>
        public const string circularProgressUssClassName = ussClassName + "__circular-progress";

        /// <summary>
        /// The Preloader's logo USS class name.
        /// </summary>
        public const string logoUssClassName = ussClassName + "__logo";

        /// <summary>
        /// Constructor.
        /// </summary>
        public Preloader()
        {
            pickingMode = PickingMode.Ignore;

            AddToClassList(ussClassName);

            var progress = new CircularProgress
            {
                innerRadius = 0.49f,
                pickingMode = PickingMode.Ignore
            };
            progress.AddToClassList(circularProgressUssClassName);

            hierarchy.Add(progress);

            var logo = new Image
            {
                pickingMode = PickingMode.Ignore
            };
            logo.AddToClassList(logoUssClassName);

            progress.Add(logo);
        }

    }
}
