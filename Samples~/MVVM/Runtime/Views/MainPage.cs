using System.ComponentModel;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;
using Unity.Properties;

namespace Unity.AppUI.Samples.MVVM
{
    public class MainPage : VisualElement
    {
        readonly Text m_Text;
        readonly Button m_Button;
        readonly MainViewModel m_BindingContext;

        public MainPage(MainViewModel viewModel)
        {
            style.marginBottom = 100;
            style.marginLeft = 100;
            style.marginRight = 100;
            style.marginTop = 100;

            m_BindingContext = viewModel;
            m_Text = new Text("Click count: 0");
            m_Button = new Button(m_BindingContext.IncrementCounterCommand.Execute) { title = "Increment" };
            Add(m_Text);
            Add(m_Button);

            // Starting Unity 2023.2, we can use UITK data binding to update the text.
            dataSource = m_BindingContext;
            m_Text.SetBinding(nameof(Text.text), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = PropertyPath.FromName(nameof(MainViewModel.ClickCountMessage))
            });
        }
    }
}
