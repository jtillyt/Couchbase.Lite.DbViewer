using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace DbViewer.TreeView
{
    public class TreeViewNode : StackLayout
    {
        #region Internal Fields

        internal readonly BoxView SelectionBoxView = new BoxView {Color = Color.Blue, Opacity = .5, IsVisible = false};

        #endregion

        #region Image source for icons

        #endregion

        #region Protected Overrides

        protected override void OnParentSet()
        {
            base.OnParentSet();
            Render();
        }

        #endregion

        #region Fields

        private TreeViewNode _ParentTreeViewItem;

        private DateTime _ExpandButtonClickedTime;

        private readonly BoxView _SpacerBoxView = new BoxView();
        private readonly BoxView _EmptyBox = new BoxView {BackgroundColor = Color.Blue, Opacity = .5};


        private const int ExpandButtonWidth = 32;
        private readonly ContentView _ExpandButtonContent = new ContentView();

        private readonly Grid _MainGrid = new Grid
        {
            VerticalOptions = LayoutOptions.StartAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
            RowSpacing = 2
        };

        private readonly StackLayout _ContentStackLayout = new StackLayout {Orientation = StackOrientation.Horizontal};

        private readonly ContentView _ContentView = new ContentView
        {
            HorizontalOptions = LayoutOptions.FillAndExpand
        };

        private readonly StackLayout _ChildrenStackLayout = new StackLayout
        {
            Orientation = StackOrientation.Vertical,
            Spacing = 0,
            IsVisible = false
        };

        private IList<TreeViewNode> _Children = new ObservableCollection<TreeViewNode>();
        private readonly TapGestureRecognizer _TapGestureRecognizer = new TapGestureRecognizer();
        private readonly TapGestureRecognizer _ExpandButtonGestureRecognizer = new TapGestureRecognizer();

        private readonly TapGestureRecognizer _DoubleClickGestureRecognizer = new TapGestureRecognizer();

        #endregion

        #region Private Properties

        private TreeView ParentTreeView => Parent?.Parent as TreeView;
        private double IndentWidth => Depth * SpacerWidth;
        private int SpacerWidth { get; } = 30;
        private int Depth => ParentTreeViewItem?.Depth + 1 ?? 0;

        #endregion

        #region Events

        public event EventHandler Expanded;

        /// <summary>
        ///     Occurs when the user double clicks on the node
        /// </summary>
        public event EventHandler DoubleClicked;

        #endregion

        #region Public Properties

        public bool IsSelected
        {
            get => SelectionBoxView.IsVisible;
            set => SelectionBoxView.IsVisible = value;
        }

        public bool IsExpanded
        {
            get => _ChildrenStackLayout.IsVisible;
            set
            {
                _ChildrenStackLayout.IsVisible = value;

                Render();
                if (value)
                {
                    Expanded?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        ///     set to true to show the expand button in case we need to poulate the child nodes on demand
        /// </summary>
        public bool ShowExpandButtonIfEmpty { get; set; } = false;

        /// <summary>
        ///     set BackgroundColor when node is tapped/selected
        /// </summary>
        public Color SelectedBackgroundColor { get; set; } = Color.Blue;

        /// <summary>
        ///     SelectedBackgroundOpacity when node is tapped/selected
        /// </summary>
        public double SelectedBackgroundOpacity { get; set; } = .3;

        /// <summary>
        ///     customize expand icon based on isExpanded property and or data
        /// </summary>
        public DataTemplate ExpandButtonTemplate { get; set; } = null;

        public View Content
        {
            get => _ContentView.Content;
            set => _ContentView.Content = value;
        }

        public IList<TreeViewNode> Children
        {
            get => _Children;
            set
            {
                if (_Children is INotifyCollectionChanged notifyCollectionChanged)
                {
                    notifyCollectionChanged.CollectionChanged -= ItemsSource_CollectionChanged;
                }

                _Children = value;

                if (_Children is INotifyCollectionChanged notifyCollectionChanged2)
                {
                    notifyCollectionChanged2.CollectionChanged += ItemsSource_CollectionChanged;
                }

                TreeView.RenderNodes(_Children, _ChildrenStackLayout,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset), this);

                Render();
            }
        }

        /// <summary>
        ///     TODO: Remove this. We should be able to get the ParentTreeViewNode by traversing up through the Visual Tree by
        ///     'Parent', but this not working for some reason.
        /// </summary>
        public TreeViewNode ParentTreeViewItem
        {
            get => _ParentTreeViewItem;
            set
            {
                _ParentTreeViewItem = value;
                Render();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        ///     Constructs a new TreeViewItem
        /// </summary>
        public TreeViewNode()
        {
            var itemsSource = (ObservableCollection<TreeViewNode>) _Children;
            itemsSource.CollectionChanged += ItemsSource_CollectionChanged;

            _TapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            GestureRecognizers.Add(_TapGestureRecognizer);

            _MainGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            _MainGrid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            _MainGrid.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});

            _MainGrid.Children.Add(SelectionBoxView);

            _ContentStackLayout.Children.Add(_SpacerBoxView);
            _ContentStackLayout.Children.Add(_ExpandButtonContent);
            _ContentStackLayout.Children.Add(_ContentView);

            SetExpandButtonContent(ExpandButtonTemplate);

            _ExpandButtonGestureRecognizer.Tapped += ExpandButton_Tapped;
            _ExpandButtonContent.GestureRecognizers.Add(_ExpandButtonGestureRecognizer);

            _DoubleClickGestureRecognizer.NumberOfTapsRequired = 2;
            _DoubleClickGestureRecognizer.Tapped += DoubleClick;
            _ContentView.GestureRecognizers.Add(_DoubleClickGestureRecognizer);


            _MainGrid.Children.Add(_ContentStackLayout);
            _MainGrid.Children.Add(_ChildrenStackLayout, 0, 1);

            base.Children.Add(_MainGrid);

            HorizontalOptions = LayoutOptions.FillAndExpand;
            VerticalOptions = LayoutOptions.Start;

            Render();
        }

        private void _DoubleClickGestureRecognizer_Tapped(object sender, EventArgs e)
        {
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     TODO: This is a little stinky...
        /// </summary>
        private void ChildSelected(TreeViewNode child)
        {
            //Um? How does this work? The method here is a private method so how are we calling it?
            ParentTreeViewItem?.ChildSelected(child);
            ParentTreeView?.ChildSelected(child);
        }

        private void Render()
        {
            _SpacerBoxView.WidthRequest = IndentWidth;

            if ((Children == null || Children.Count == 0) && !ShowExpandButtonIfEmpty)
            {
                SetExpandButtonContent(ExpandButtonTemplate);
                return;
            }

            SetExpandButtonContent(ExpandButtonTemplate);

            foreach (var item in Children)
            {
                item.Render();
            }
        }

        /// <summary>
        ///     Use DataTemplae
        /// </summary>
        private void SetExpandButtonContent(DataTemplate expandButtonTemplate)
        {
            if (expandButtonTemplate != null)
            {
                _ExpandButtonContent.Content = (View) expandButtonTemplate.CreateContent();
            }
            else
            {
                _ExpandButtonContent.Content = new ContentView {Content = _EmptyBox};
            }
        }

        #endregion

        #region Event Handlers

        private void ExpandButton_Tapped(object sender, EventArgs e)
        {
            _ExpandButtonClickedTime = DateTime.Now;
            IsExpanded = !IsExpanded;
        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            //TODO: Hack. We don't want the node to become selected when we are clicking on the expanded button
            if (DateTime.Now - _ExpandButtonClickedTime > new TimeSpan(0, 0, 0, 0, 50))
            {
                ChildSelected(this);
            }
        }


        private void DoubleClick(object sender, EventArgs e)
        {
            DoubleClicked?.Invoke(this, new EventArgs());
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TreeView.RenderNodes(_Children, _ChildrenStackLayout, e, this);
            Render();
        }

        #endregion
    }
}