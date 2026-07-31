using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A container component that provides contextual settings to its children in the UI hierarchy.
    /// </summary>
    /// <remarks>
    /// The ContextProvider is a powerful layout component that allows you to override various UI settings for a
    /// specific section of your interface. It acts as a wrapper around UI elements, providing contextual
    /// configurations that affect all child elements within its scope.
    ///
    /// This component is particularly useful when you need to create isolated sections of your UI with different
    /// themes, scales, languages, or layout directions without affecting the rest of the application.
    ///
    /// Note: The ContextProvider's picking mode is set to ignore by default, meaning it won't intercept any
    /// pointer events. This allows events to pass through to its children.
    /// </remarks>
    /// <example>
    /// <para>Here are some common usage examples of the ContextProvider component. Creating a dark-themed modal dialog:</para>
    /// <code lang="xml"><![CDATA[
    /// <ContextProvider theme="dark" scale="medium">
    ///     <Dialog>
    ///         <Header text="Dark Theme Dialog" />
    ///         <Content>
    ///             <Label text="This entire dialog uses dark theme styling" />
    ///         </Content>
    ///         <Footer>
    ///             <Button text="Close" />
    ///         </Footer>
    ///     </Dialog>
    /// </ContextProvider>
    /// ]]></code>
    /// <para>Setting up a multilingual section with RTL support.</para>
    /// <code lang="xml"><![CDATA[
    /// <ContextProvider lang="ar-SA" dir="Rtl">
    ///     <Panel class="form-section">
    ///         <TextField label="الاسم" />
    ///         <TextField label="البريد الإلكتروني" />
    ///         <Button text="إرسال" />
    ///     </Panel>
    /// </ContextProvider>
    /// ]]></code>
    /// <para>Creating a larger scale section for improved accessibility.</para>
    /// <code lang="xml"><![CDATA[
    /// <ContextProvider scale="large" tooltip-delay-ms="0">
    ///     <Panel class="accessibility-section">
    ///         <Label text="Larger Text for Better Visibility" />
    ///         <Button text="Easy to Click" tooltip="Instant tooltip" />
    ///     </Panel>
    /// </ContextProvider>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("context-components")]
    public partial class ContextProvider : BaseVisualElement
    {
        /// <summary>
        /// Main Uss Class Name.
        /// </summary>
        public const string ussClassName = "appui-context-provider";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContextProvider()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;
        }

    }
}