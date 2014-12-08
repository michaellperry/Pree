using System.Windows;
using System.Windows.Controls;

namespace Pree.Converters
{
    class ViewSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var wrapper = item as UpdateControls.XAML.Wrapper.IObjectInstance;
            var element = container as FrameworkElement;
            if (wrapper != null && element != null)
            {
                for (var type = wrapper.WrappedObject.GetType(); type != null && type != typeof(object); type = type.BaseType)
                {
                    var template = element.TryFindResource(new DataTemplateKey(type)) as DataTemplate;
                    if (template != null)
                        return template;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
