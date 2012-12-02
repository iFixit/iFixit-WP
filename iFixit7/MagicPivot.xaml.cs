using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

namespace iFixit7
{
    public partial class MagicPivot : PhoneApplicationPage
    {
        private MagicPivotViewModel vm {get; set;}

        //private String navParentName = "";
        //private String navSelectedName = "";
        //private String navSelectedType = "";

        public const string MagicTypeCategory = "category";
        public const string MagicTypeTopic = "topic";

        public Category thisCat = null;

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");
        }

        private void setupBinding(Category c)
        {
            //instantiate view model
            vm = new MagicPivotViewModel(c, SmartPivot);

            this.DataContext = vm;
            vm.Columns[vm.TabIndex].setColumnContent();
            SmartPivot.LoadingPivotItem += new EventHandler<PivotItemEventArgs>(SmartPivot_LoadingPivotItem);
        }


        void tb_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //figure out if the sender is a text block (it should be). Filters out tapping whitespace.
            TextBlock t = new TextBlock();

            //string selected = (e.OriginalSource as TextBlock).Text as String;
            var selectedItem = (sender as ListBox).SelectedItem;
            if( selectedItem == null)
                return;

            string selected = (selectedItem as MagicPivotViewModel.ColumnContent.InnerColumnContent).Tag as String;
            Debug.WriteLine("MagicPivot tapped > [" + selected + "]");

            //get the parent
            MagicPivotViewModel.ColumnContent col = vm.Columns[vm.TabIndex];
            string parent = col.ColumnHeader;
            string type = col.findType(selected);

            //if we are about to navigate to a topic (leaf), navigate to a device info page
            if (type == MagicPivot.MagicTypeTopic)
            {
                Debug.WriteLine("Saving " + col.colCat.Name + " at Key: " + col.colCat.Parent.Name);
                PhoneApplicationService.Current.State[col.colCat.Parent.Name] = col.colCat;
                NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + selected,
                UriKind.Relative));
            }
            else
            {
                Category temp = col.colCat.Categories.FirstOrDefault(c => c.Name == selected);
                
                Debug.WriteLine("Saving " + temp.Name + " at Key: " + temp.Parent.Name);
                PhoneApplicationService.Current.State[temp.Parent.Name] = temp;

                Debug.WriteLine("Saving " + temp.Parent.Name + " at Key: " + vm.selectedCategory.Parent.Name);
                PhoneApplicationService.Current.State[vm.selectedCategory.Parent.Name] = temp.Parent;

                //NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + parent +
                //    "&SelectedCategory=" + selected +
                //    "&SelectedType=" + type,
                //    UriKind.Relative));
                NavigationService.Navigate(new Uri("/MagicPivot.xaml?parent=" + temp.Parent.Name, UriKind.Relative));
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.State[vm.selectedCategory.Name] = SmartPivot.SelectedIndex;
        }

        /*
         * Called right as we are being navigated to
         */
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Debug.WriteLine("A MagicPivot has been navigated to...");

            string navParentName = NavigationContext.QueryString[App.MagicParentTag];
            Debug.WriteLine("Looking for Key: " + navParentName);

            if (PhoneApplicationService.Current.State.ContainsKey(navParentName))
                this.thisCat = (Category)PhoneApplicationService.Current.State[navParentName];
            Debug.WriteLine("Saving in current.state: " + this.thisCat);
            Debug.WriteLine("I'm going to try to pull a category out: " + thisCat.Name);

            setupBinding(thisCat);
            string key = vm.selectedCategory.Name;
            if (key != null && this.State.ContainsKey(key))
            {
                int selectedTabIndex = (int)this.State[key];
                if (0 <= selectedTabIndex && selectedTabIndex < vm.Columns.Count)
                {
                    SmartPivot.SelectedIndex = selectedTabIndex;
                }
            }
        }

        /*
         * These two are the handlers for the application bar buttons
         */
        private void AppBarSearch_Click(object sender, EventArgs e)
        {
            // error out and go back if there is no netowork connection
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                MessageBox.Show("Search cannot be used without an internet connection.");
                return;
            }
            NavigationService.Navigate(new Uri("/SearchView.xaml", UriKind.Relative));
        }
        private void AppBarFavorites_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FavoriteItems.xaml", UriKind.Relative));
        }

        public void SmartPivot_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            //throw new NotImplementedException();


            PivotItem p = e.Item;
            MagicPivotViewModel.ColumnContent cc = p.Content as MagicPivotViewModel.ColumnContent;

            if (cc.ColContent.Count > 0 || cc.TypeRefs.Count > 0)
                return;

            cc.setColumnContent();

            vm.TabIndex = SmartPivot.SelectedIndex;
            Debug.WriteLine("Set content for " + cc.ColumnHeader + ". got " + cc.ColContent.Count + " items");
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            MagicPivotViewModel.ColumnContent col = vm.Columns[vm.TabIndex];
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = col.colCat.url;
            wbt.Show();
        }

    }

    public class MagicPivotViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /*
         * what goes inside each column in the collection of them
         */
        public class ColumnContent : INotifyPropertyChanged, INotifyPropertyChanging
        {
            //this extra internal layer is heinous and makes me feed bad
            public class InnerColumnContent
            {
                public InnerColumnContent(string name, string tag)
                {
                    this.Name = name;
                    this.Tag = tag;
                }

                private string _name = "";
                public string Name
                {
                    get
                    {
                        return _name;
                    }
                    set
                    {
                        if (_name != value)
                        {
                            _name = value;
                        }
                    }
                }

                private string _tag = "";
                public string Tag
                {
                    get
                    {
                        return _tag;
                    }
                    set
                    {
                        if (_tag != value)
                        {
                            _tag = value;
                        }
                    }
                }
            }

            private string _columnHeader = "";
            //FIXME need to notify changed?
            public string ColumnHeader
            {
                get
                {
                    return _columnHeader;
                }
                set
                {
                    if (_columnHeader != value)
                    {
                        NotifyPropertyChanging("ColumnHeader");
                        _columnHeader = value;
                        NotifyPropertyChanged("ColumnHeader");
                    }
                }
            }

            private List<InnerColumnContent> _colContent = new List<InnerColumnContent>();
            public List<InnerColumnContent> ColContent
            {
                get
                {
                    return _colContent;
                }
                set
                {
                    if (_colContent != value)
                    {
                        NotifyPropertyChanging("ColContent");
                        _colContent = value;
                        NotifyPropertyChanged("ColContent");
                    }
                }
            }

            public Category colCat = null;

            public Dictionary<string, string> TypeRefs;
            private bool HasBegunLoading = false;

            public ColumnContent(Category c)
            {
                this.colCat = c;
                this.ColumnHeader = c.Name;

                ColContent = new List<InnerColumnContent>();

                TypeRefs = new Dictionary<string, string>();
            }

            /*
             * A strange hack that takes a string, figures out if its a category or topic, and returns a string
             * indicating which
             */
            public string findType(string q)
            {
                return TypeRefs[q];
            }

            public void setColumnContent()
            {
                if (HasBegunLoading || ColContent.Count > 0)
                    return;
                HasBegunLoading = true;

                //add all sub categories
                foreach (Category c in colCat.Categories)
                {
                    //Debug.WriteLine("col content " + c.Name);
                    NotifyPropertyChanging("ColContent");
                    //ColContent.Add(c.Name);
                    ColContent.Add(new InnerColumnContent(c.ShortName, c.Name));
                    NotifyPropertyChanged("ColContent");
                    TypeRefs.Add(c.Name, MagicPivot.MagicTypeCategory);
                }

                //add all topics
                foreach (Topic t in colCat.Topics)
                {
                    //Debug.WriteLine("col content topic " + t.Name);

                    NotifyPropertyChanging("ColContent");
                    //ColContent.Add(t.Name);
                    ColContent.Add(new InnerColumnContent(t.ShortName, t.Name));
                    NotifyPropertyChanged("ColContent");
                    TypeRefs.Add(t.Name, MagicPivot.MagicTypeTopic);
                }

                //FIXME sort by .Tag?
                var q = from cc in ColContent
                        orderby cc.Name
                        select cc;
                ColContent = q.ToList();
                HasBegunLoading = false;
            }
            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            // Used to notify the page that a data context property changed
            private void NotifyPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    });
                }
            }

            #endregion

            #region INotifyPropertyChanging Members

            public event PropertyChangingEventHandler PropertyChanging;

            // Used to notify the data context that a data context property is about to change
            private void NotifyPropertyChanging(string propertyName)
            {
                if (PropertyChanging != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
                    });
                }
            }

            #endregion
        }

        private int _tabIndex;
        //column index
        public int TabIndex {
            get
            {
                return _tabIndex;
            }
            set
            {
                if (_tabIndex != value)
                {
                    NotifyPropertyChanging("TabIndex");
                    _tabIndex = value;
                    NotifyPropertyChanged("TabIndex");
                }
            }
        }

        private List<ColumnContent> _columns = new List<ColumnContent>();
        //collection of column content
        public List<ColumnContent> Columns
        {
            get
            {
                return _columns;
            }
            set
            {

                if (_columns != value)
                {
                    NotifyPropertyChanging("Columns");
                    _columns = value;
                    NotifyPropertyChanged("Columns");
                }
            }
        }

        //names
        //FIXME need to notify changed
        // ParentName and Selected name have been replaced by selectedCategory. 
        // This category represents the selected column, and this category's parent is the
        // category that will populate the entire PivotView (other columns)
        //public string ParentName { get; set; }
        //public string SelectedName { get; set; }
        public Category selectedCategory { get; set; }
        public Pivot myPivot { get; set; }

        //public MagicPivotViewModel(string pName, string selName, Pivot sp)
        public MagicPivotViewModel(Category selectedCategory, Pivot sp)
        {
            TabIndex = 0;
            Columns = new List<ColumnContent>();

            //this.ParentName = pName;
            //this.SelectedName = selName;
            this.selectedCategory = selectedCategory;

            myPivot = sp;

            Debug.WriteLine("Current category is " + this.selectedCategory.Name + " who's parent is " + this.selectedCategory.Parent.Name);
            //Debug.WriteLine("Made a new MagicPivot View Model with parent = " + pName + " & cur = " + selName);

            //force all data to update
            //if (ParentName != "" && Columns.Count == 0)
            if (Columns.Count == 0)
                UpdateData();
        }

        private void UpdateDataAsync()
        {
            var _Worker = new BackgroundWorker();
            _Worker.DoWork += (s, e) =>
                { UpdateData(); };
            _Worker.RunWorkerAsync();
        }

        /*
         * Update the data and index
         */
        public void UpdateData()
        {
            //run a query to fill the list of column headers
            //FIXME need to notify changed
            Category parentCat = this.selectedCategory.Parent;
            //using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            //{
            //    parentCat = DBHelpers.GetCompleteCategory(ParentName, db);
            //    /*
            //    IQueryable<Category> query =
            //        from cats in App.mDB.CategoriesTable
            //        where cats.Name == ParentName
            //        select cats;
            //     */
            //}
            int index = 0;
            foreach (Category c in parentCat.Categories)
            {
                Debug.WriteLine("Got Cat " + c.Name);

                ColumnContent cc = new ColumnContent(c);
                NotifyPropertyChanging("Columns");
                this.Columns.Add(cc);
                NotifyPropertyChanged("Columns");
                //cc.setColumnContent();

                //if (c.Name == SelectedName)
                if (c.Name == this.selectedCategory.Name)
                {
                    NotifyPropertyChanging("TabIndex");
                    TabIndex = index;
                    NotifyPropertyChanged("TabIndex");
                }
                index++;
            }

            Debug.WriteLine("tab index = " + TabIndex);

            var sort = from col in Columns
                      orderby col.ColumnHeader
                      select col;
            Columns = sort.ToList();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }
    
}