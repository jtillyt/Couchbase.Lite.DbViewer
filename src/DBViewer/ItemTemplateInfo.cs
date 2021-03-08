#if(WPFSILVERLIGHT)
using System.Windows;
#else
using Xamarin.Forms;
#endif
using System.Linq;

namespace Adapt.Presentation.Controls
{
    public class ItemTemplateInfo
    {
        public DataTemplate ItemTemplate { get; set; }
        public string TypeName { get; set; }
        public string SortPropertyPath { get; set; }
    }
}