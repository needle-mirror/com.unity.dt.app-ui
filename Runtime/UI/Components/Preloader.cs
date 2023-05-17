using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A preloader visual element.
    /// </summary>
    public class Preloader : VisualElement
    {
        /// <summary>
        /// The Preloader's USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-preloader";
        
        /// <summary>
        /// The Preloader's circular progress USS class name.
        /// </summary>
        public static readonly string circularProgressUssClassName = ussClassName + "__circular-progress";
        
        /// <summary>
        /// The Preloader's logo USS class name.
        /// </summary>
        public static readonly string logoUssClassName = ussClassName + "__logo";
        
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
        
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Preloader, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="AccordionItem"/>.
        /// </summary>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            
        }
    }
}
