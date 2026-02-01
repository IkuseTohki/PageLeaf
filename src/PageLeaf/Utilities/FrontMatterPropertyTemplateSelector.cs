using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Utilities
{
    public class FrontMatterPropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DefaultTemplate { get; set; }
        public DataTemplate? TagsTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is FrontMatterProperty prop)
            {
                if (prop.IsTags)
                {
                    return TagsTemplate;
                }
                return DefaultTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
