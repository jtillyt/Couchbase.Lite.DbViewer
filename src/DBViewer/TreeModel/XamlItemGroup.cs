using System;
using System.Collections.Generic;
using System.Linq;

//Taken from https://github.com/AdaptSolutions/Xamarin.Forms-TreeView
namespace DbViewer.TreeModel
{
    [Serializable]
    public class XamlItemGroup
    {
        public List<XamlItemGroup> Children { get; } = new List<XamlItemGroup>();
        public List<XamlItem> XamlItems { get; } = new List<XamlItem>();
        public string Name { get; set; }
    }

    [Serializable]
    public class XamlItem
    {
        public string Key { get; set; }
    }
}