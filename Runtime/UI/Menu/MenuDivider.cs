using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A horizontal line that separates menu items into distinct groups.
    /// </summary>
    /// <remarks>
    /// MenuDivider is a specialized version of the Divider component designed specifically for use within Menu
    /// elements. It creates a visual separation between menu items, helping to organize actions into logical
    /// groups.
    ///
    /// Menu dividers inherit all properties from the base Divider component, including direction, size, and
    /// spacing, but are styled specifically for menu contexts with appropriate margins and appearance.
    ///
    /// Use dividers to separate different types of actions or to create visual breaks between related groups of
    /// menu items. This improves menu scanability and helps users quickly locate the action they need.
    ///
    /// Note: MenuDivider is always horizontal and is non-interactive (picking mode set to ignore).
    /// </remarks>
    /// <example>
    /// <para>Basic Menu with Dividers</para>
    ///
    /// <para>Using dividers to separate logical groups of actions.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuItem label="New" icon="file" />
    ///     <ui:MenuItem label="Open" icon="folder-open" />
    ///     <ui:MenuDivider />
    ///     <ui:MenuItem label="Save" icon="save" />
    ///     <ui:MenuItem label="Save As" />
    ///     <ui:MenuDivider />
    ///     <ui:MenuItem label="Exit" icon="exit" />
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Dividers Between Sections</para>
    ///
    /// <para>Using dividers to separate major sections in a complex menu.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuSection title="Edit">
    ///         <ui:MenuItem label="Undo" shortcut="Ctrl+Z" />
    ///         <ui:MenuItem label="Redo" shortcut="Ctrl+Y" />
    ///     </ui:MenuSection>
    ///     <ui:MenuDivider />
    ///     <ui:MenuSection title="Selection">
    ///         <ui:MenuItem label="Select All" shortcut="Ctrl+A" />
    ///         <ui:MenuItem label="Deselect" />
    ///     </ui:MenuSection>
    ///     <ui:MenuDivider />
    ///     <ui:MenuSection title="View">
    ///         <ui:MenuItem label="Zoom In" shortcut="Ctrl++" />
    ///         <ui:MenuItem label="Zoom Out" shortcut="Ctrl+-" />
    ///     </ui:MenuSection>
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Creating Dividers Programmatically</para>
    ///
    /// <para>Building menus with dividers in code, including custom styling.</para>
    /// <code lang="csharp"><![CDATA[
    /// var menu = new Menu();
    ///
    /// // Add file operations
    /// menu.Add(new MenuItem { label = "New", icon = "file" });
    /// menu.Add(new MenuItem { label = "Open", icon = "folder-open" });
    ///
    /// // Add divider
    /// menu.Add(new MenuDivider());
    ///
    /// // Add edit operations
    /// menu.Add(new MenuItem { label = "Cut", icon = "scissors" });
    /// menu.Add(new MenuItem { label = "Copy", icon = "copy" });
    /// menu.Add(new MenuItem { label = "Paste", icon = "clipboard" });
    ///
    /// // Add another divider with custom styling
    /// var divider = new MenuDivider {
    ///     size = Size.S,
    ///     spacing = Spacing.L
    /// };
    /// menu.Add(divider);
    /// ]]></code>
    /// <para>Subtle vs Prominent Dividers</para>
    ///
    /// <para>Using different divider styles to create varying levels of visual separation.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuItem label="Common Action 1" />
    ///     <ui:MenuItem label="Common Action 2" />
    ///     <!-- Subtle divider for minor separation -->
    ///     <ui:MenuDivider size="XS" spacing="S" />
    ///     <ui:MenuItem label="Less Common Action 1" />
    ///     <ui:MenuItem label="Less Common Action 2" />
    ///     <!-- Prominent divider for major separation -->
    ///     <ui:MenuDivider size="M" spacing="L" />
    ///     <ui:MenuItem label="Destructive Action" icon="trash" />
    /// </ui:Menu>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups", id = "menu-divider", displayName = "Menu Divider")]
    public partial class MenuDivider : Divider
    {
        /// <summary>
        /// The MenuDivider main styling class.
        /// </summary>
        public const string dividerClassName = "appui-menu__divider";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuDivider()
        {
            AddToClassList(dividerClassName);
        }

    }
}
