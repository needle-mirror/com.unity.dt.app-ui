using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.Core;
#if UNITY_LOCALIZATION_PRESENT
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A text element that supports localization and string formatting with variables.
    /// </summary>
    /// <remarks>
    /// LocalizedTextElement is a specialized text component that enables seamless localization of text content
    /// in your Unity application's UI. It extends BaseTextElement and provides built-in support for string
    /// localization and dynamic text formatting using variables.
    ///
    /// This component is particularly useful when building applications that need to support multiple
    /// languages and require dynamic text updates based on user interactions or application state.
    ///
    /// The component integrates with Unity's Localization system and supports both direct text display and
    /// localized string references. When using localized strings, it automatically updates the displayed text
    /// when the application's language changes.
    ///
    /// Note: To use the localization features, make sure you have the Unity Localization package installed in
    /// your project. Without it, the component will function as a regular text element.
    /// </remarks>
    /// <example>
    /// <para>Basic Usage with Plain Text.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ui:LocalizedTextElement text="Hello World" />
    /// </UXML>
    /// ]]></code>
    /// <para>Using Localization Keys.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ui:LocalizedTextElement text="UI.Welcome" />
    /// </UXML>
    ///
    /// // C#
    /// var welcomeText = new LocalizedTextElement("UI.Welcome");
    /// container.Add(welcomeText);
    /// ]]></code>
    /// <para>Dynamic Text with Variables.</para>
    /// <code lang="csharp"><![CDATA[
    /// // Assuming the localized string is "Player {0} scored {1} points!"
    /// var scoreText = new LocalizedTextElement("UI.ScoreMessage");
    /// scoreText.variables = new object[] { "Player1", 100 };
    /// container.Add(scoreText);
    /// ]]></code>
    /// <para>Complete Example with Styling.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <ui:LocalizedTextElement
    ///     text="UI.WelcomeMessage"
    ///     class="title-text"
    ///     style="font-size: 20px;" />
    /// </UXML>
    ///
    /// // C#
    /// var welcomeText = new LocalizedTextElement("UI.WelcomeMessage");
    /// welcomeText.variables = new object[] { "User", DateTime.Now.ToString() };
    /// welcomeText.AddToClassList("title-text");
    /// welcomeText.style.fontSize = 20;
    /// container.Add(welcomeText);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("typography")]
    public partial class LocalizedTextElement : BaseTextElement
    {

        internal static readonly BindingId textProperty = new BindingId(nameof(text));

        internal static readonly BindingId variablesProperty = new BindingId(nameof(variables));


        /// <summary>
        /// The main USS class name of this element.
        /// </summary>
        public new const string ussClassName = "appui-localized-text";

        static readonly CustomStyleProperty<int> k_FontWeightProperty = new ("--unity-font-weight");

        string m_ReferenceText;

        IList<object> m_Variables;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LocalizedTextElement()
            : this(null) { }

        /// <summary>
        /// Constructor with a reference text.
        /// </summary>
        /// <param name="text"> The reference text to use when formatting the localized string. You can also use plain text for no translation. </param>
        public LocalizedTextElement(string text)
        {
            AddToClassList(ussClassName);

            this.text = text;

            this.RegisterContextChangedCallback<LangContext>(OnLangContextChanged);
        }

        /// <summary>
        /// The reference text to use when formatting the localized string. You can also use plain text for no translation.
        /// </summary>
        [CreateProperty]
        public new string text
        {
            get => m_ReferenceText;
            set
            {
                var changed = m_ReferenceText != value;
                if (changed)
                {
                    m_ReferenceText = value;
                    _ = UpdateTextWithCurrentLocale();
                    NotifyPropertyChanged(in textProperty);
                }
            }
        }

        internal string localizedText
        {
            get => base.text;
            private set
            {
                if (base.text != value)
                    base.text = value;
            }
        }

        [UxmlAttribute("text")]
        string textOverride
        {
            get => this.text;
            set => this.text = value;
        }

        /// <summary>
        /// The variables to use when formatting the localized string.
        /// </summary>
        [CreateProperty]
        public IList<object> variables
        {
            get => m_Variables;
            set
            {
                var changed = m_Variables != value;
                m_Variables = value;
                _ = UpdateTextWithCurrentLocale();

                if (changed)
                    NotifyPropertyChanged(in variablesProperty);
            }
        }

        void OnLangContextChanged(ContextChangedEvent<LangContext> evt)
        {
            _ = UpdateTextWithCurrentLocale();
        }

        async Task UpdateTextWithCurrentLocale()
        {
            if (!LocalizationUtils.TryGetTableAndEntry(m_ReferenceText, out _, out _)
                || this.GetContext<LangContext>() is not {} ctx)
            {
                localizedText = m_ReferenceText;
                return;
            }

            localizedText = await ctx.GetLocalizedStringAsync(m_ReferenceText, m_Variables?.ToArray());
        }

    }
}
