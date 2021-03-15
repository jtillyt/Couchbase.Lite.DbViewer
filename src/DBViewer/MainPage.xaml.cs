using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using DBViewer.TreeView;
using Xamarin.Forms;

namespace DBViewer
{
    public partial class MainPage : ContentPage
    {
        public readonly ObservableCollection<TreeViewNode> _rootNodes = new ObservableCollection<TreeViewNode>();

        //Fix container
        public MainPage()
        {
            InitializeComponent();

            ViewModel = new MainViewModel();

            ViewModel.RootNodes.CollectionChanged += RootNodes_CollectionChanged;
            BindingContext = ViewModel;

            DocTreeView.RootNodes = _rootNodes;
        }

        public MainViewModel ViewModel { get; }

        private void RootNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var groupVm = e.NewItems[0] as DocumentGroupViewModel;

            var groupTemplate = Resources["GroupNodeTemplate"] as ControlTemplate;
            var groupTreeNode = groupTemplate.CreateContent() as TreeViewNode;
            groupTreeNode.BindingContext = groupVm;
            groupTreeNode.Expanded += GroupTreeNode_Expanded;

            _rootNodes.Add(groupTreeNode);
        }

        private void GroupTreeNode_Expanded(object sender, EventArgs e)
        {
            var groupTreeViewNode = sender as TreeViewNode;

            if (!(groupTreeViewNode?.BindingContext is DocumentGroupViewModel groupVm))
                return;

            if (groupTreeViewNode.Children.Count > 0)
                return;

            var documentTreeNodes = new List<TreeViewNode>();

            foreach (var docVm in groupVm.Documents)
            {
                var docTemplate = Resources["DocumentNodeTemplate"] as ControlTemplate;
                var docTreeNode = docTemplate.CreateContent() as TreeViewNode;
                docTreeNode.BindingContext = docVm;
                documentTreeNodes.Add(docTreeNode);

                docTreeNode.DoubleClicked += DocTreeNode_DoubleClicked;
            }

            groupTreeViewNode.Children = documentTreeNodes;
        }

        private void DocTreeNode_DoubleClicked(object sender, EventArgs e)
        {
            var docTreeViewNode = sender as TreeViewNode;

            if (!(docTreeViewNode?.BindingContext is DocumentViewModel docVm))
                return;

            var docJson = docVm.GetJson();
            ViewModel.JsonText = docJson;
        }
    }
}