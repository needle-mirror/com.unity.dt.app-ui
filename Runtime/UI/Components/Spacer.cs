using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The spacer spacing.
    /// </summary>
    public enum SpacerSpacing
    {
        /// <summary>
        /// No spacing.
        /// </summary>
        Null,
        
        /// <summary>
        /// Extra small spacing.
        /// </summary>
        XS,
        
        /// <summary>
        /// Small spacing.
        /// </summary>
        S,
        
        /// <summary>
        /// Medium spacing.
        /// </summary>
        M,
        
        /// <summary>
        /// Large spacing.
        /// </summary>
        L,
        
        /// <summary>
        /// Extra large spacing.
        /// </summary>
        XL,
        
        /// <summary>
        /// The spacer will expand to fill the remaining space using flex-grow.
        /// </summary>
        Expand        
    }
    
    /// <summary>
    /// Spacer visual element.
    /// </summary>
    public class Spacer : VisualElement
    {
        const SpacerSpacing k_DefaultSpacing = SpacerSpacing.M;
        
        /// <summary>
        /// The spacer's main USS class name.
        /// </summary>
        public static readonly string ussClassName = "appui-spacer";
        
        /// <summary>
        /// The spacer's spacing USS class name.
        /// </summary>
        public static readonly string spacingUssClassName = ussClassName + "--spacing-";

        SpacerSpacing m_Spacing = k_DefaultSpacing;

        /// <summary>
        /// Main constructor.
        /// </summary>
        public Spacer()
        {
            pickingMode = PickingMode.Ignore;
            
            AddToClassList(ussClassName);
            
            spacing = k_DefaultSpacing;
        }

        /// <summary>
        /// The spacer's spacing.
        /// </summary>
        public SpacerSpacing spacing
        {
            get => m_Spacing;
            set
            {
                RemoveFromClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
                m_Spacing = value;
                AddToClassList(spacingUssClassName + m_Spacing.ToString().ToLower());
            }
        }
        
        /// <inheritdoc cref="UnityEngine.UIElements.UxmlFactory{Spacer,UxmlTraits}"/>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Spacer, UxmlTraits> { }
        
        /// <inheritdoc cref="UnityEngine.UIElements.VisualElement.UxmlTraits"/>
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlEnumAttributeDescription<SpacerSpacing> m_Spacing =
                new UxmlEnumAttributeDescription<SpacerSpacing>
                {
                    name = "spacing",
                    defaultValue = k_DefaultSpacing
                };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var element = (Spacer)ve;
                element.spacing = m_Spacing.GetValueFromBag(bag, cc);
            }
        }
    }
}
