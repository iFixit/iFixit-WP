using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace iFixit7
{
    public partial class MagicPivot : PhoneApplicationPage
    {
        private MagicPivotViewModel vm {get; set;}

        private String navParentName = "";
        private String navSelectedName = "";
        private String navSelectedType = "";

        public const string MagicTypeCategory = "category";
        public const string MagicTypeTopic = "topic";

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");

            this.SmartPivot.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(tb_Tap);
            setupBinding();
        }

        private void setupBinding()
        {
            //instantiate view model
            vm = new MagicPivotViewModel(navParentName, navSelectedName, SmartPivot);

            this.DataContext = vm;
            SmartPivot.LoadingPivotItem += new EventHandler<PivotItemEventArgs>(SmartPivot_LoadingPivotItem);

            //set data context to the view model
        }


        void tb_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //figure out if the sender is a text block (it should be). Filters out tapping whitespace.
            TextBlock t = new TextBlock();
            if (e.OriginalSource.GetType() != t.GetType())
            {
                return;
            }

            /*
            //make sure we arent reacting to tapping a header
            if ((string)(sender as TextBlock).Tag.ToString() == "HEADER")
            {
                return;
            }
             */

            string selected = (e.OriginalSource as TextBlock).Text as String;
            Debug.WriteLine("MagicPivot tapped > [" + selected + "]");

            //get the parent
            MagicPivotViewModel.ColumnContent col = vm.Columns[vm.TabIndex];
            string parent = col.ColumnHeader;
            string type = col.findType(selected);

            //if we are about to navigate to a topic (leaf), navigate to a device info page
            if (type == MagicPivot.MagicTypeTopic)
            {
                NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + selected,
                UriKind.Relative));
            }

            NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + parent +
                "&SelectedCategory=" + selected +
                "&SelectedType=" + type,
                UriKind.Relative));
        }

        /*
         * Called right as we are being navigated from
         */
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            //FIXME save current index in VM? alter params?
            //FIXME finish...
            //State.Add("NavigatedToHeader", (this.SmartPivot.SelectedItem as MagicPivotViewModel.ColumnContent).ColumnHeader);
        }

        /*
         * Called right as we are being navigated to
         */
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Debug.WriteLine("A MagicPivot has been navigated to...");

            navParentName = NavigationContext.QueryString[App.MagicParentTag];
            navSelectedType = NavigationContext.QueryString[App.MagicTypeTag];
            if (State.Keys.Contains("NavigatedToHeader"))
            {
                navSelectedName = State["NavigatedToHeader"] as string;
            }
            else
            {
                //get parameters
                navSelectedName = NavigationContext.QueryString[App.MagicSelectedTag];
            }

            Debug.WriteLine("magic pivot got a type = " + navSelectedType);

            setupBinding();
        }

        /*
         * These two are the handlers for the application bar buttons
         */
        private void AppBarSearch_Click(object sender, EventArgs e)
        {
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

            if (cc.ColContent.Count > 0)
                return;

            cc.setColumnContent();

            //this.DataContext = null;
            //this.DataContext = vm;

            //var thing = myPivot.DataContext;
            //myPivot.DataContext = null;
            //myPivot.DataContext = this;

            vm.TabIndex = SmartPivot.SelectedIndex;

            Debug.WriteLine("Set content for " + cc.ColumnHeader + ". got " + cc.ColContent.Count + " items");

        }

    }

    public class MagicPivotViewModel : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /*
         * what goes inside each column in the collection of them
         */
        public class ColumnContent : INotifyPropertyChanged, INotifyPropertyChanging
        {
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

            private List<string> _colContent = new List<string>();
            public List<string> ColContent
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

            //private List<string> AllCategories;
            //private List<string> AllTopics;
            private Dictionary<string, string> TypeRefs;

            public ColumnContent(Category c)
            {
                this.ColumnHeader = c.Name;

                ColContent = new List<string>();

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
                Category colCat = null;

                using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
                {
                    colCat = DBHelpers.GetCompleteCategory(ColumnHeader, db);
                    /*
                    IQueryable<Category> query =
                        from cats in App.mDB.CategoriesTable
                        where cats.Name == ColumnHeader
                        select cats;
                        */
                }
                //add all sub categories
                foreach (Category c in colCat.Categories)
                {
                    //Debug.WriteLine("col content " + c.Name);
                    NotifyPropertyChanging("ColContent");
                    ColContent.Add(c.Name);
                    NotifyPropertyChanged("ColContent");
                    TypeRefs.Add(c.Name, MagicPivot.MagicTypeCategory);
                }

                //add all topics
                foreach (Topic c in colCat.Topics)
                {
                    //Debug.WriteLine("col content topic " + c.Name);

                    NotifyPropertyChanging("ColContent");
                    ColContent.Add(c.Name);
                    NotifyPropertyChanged("ColContent");
                    TypeRefs.Add(c.Name, MagicPivot.MagicTypeTopic);
                }

                //FIXME forces sorting...
                var q = from cc in ColContent
                        orderby cc
                        select cc;
                ColContent = q.ToList();
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
        public string ParentName { get; set; }
        public string SelectedName { get; set; }
        public Pivot myPivot { get; set; }

        public MagicPivotViewModel(string pName, string selName, Pivot sp)
        {
            TabIndex = 0;
            Columns = new List<ColumnContent>();

            this.ParentName = pName;
            this.SelectedName = selName;

            myPivot = sp;

            Debug.WriteLine("Made a new MagicPivot View Model with parent = " + pName + " & cur = " + selName);

            //force all data to update
            if (ParentName != "" && Columns.Count == 0)
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
            Category parentCat = null;
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                parentCat = DBHelpers.GetCompleteCategory(ParentName, db);
                /*
                IQueryable<Category> query =
                    from cats in App.mDB.CategoriesTable
                    where cats.Name == ParentName
                    select cats;
                 */
            }
            int index = 0;
            foreach (Category c in parentCat.Categories)
            {
                //Debug.WriteLine("Got Cat " + c.Name);

                ColumnContent cc = new ColumnContent(c);
                NotifyPropertyChanging("Columns");
                this.Columns.Add(cc);
                NotifyPropertyChanged("Columns");
                //cc.setColumnContent();

                if (c.Name == SelectedName)
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