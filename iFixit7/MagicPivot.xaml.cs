using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace iFixit7
{
    public partial class MagicPivot : PhoneApplicationPage
    {
        private MagicPivotViewModel vm = null;

        private String navParentName;
        private String navSelectedName;
        private String navSelectedType;

        public const string MagicTypeCategory = "category";
        public const string MagicTypeTopic = "topic";

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");

            this.SmartPivot.Tap += new EventHandler<GestureEventArgs>(tb_Tap);
        }

        private void setupBinding()
        {
            //instantiate view model
            vm = new MagicPivotViewModel(navParentName, navSelectedName);

            //set data context to the view model
            this.DataContext = vm;
        }

        void tb_Tap(object sender, GestureEventArgs e)
        {
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

            //FIXME need to figure out what the type is!
            NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + parent +
                "&SelectedCategory=" + selected +
                "&SelectedType=" + type,
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Debug.WriteLine("A MagicPivot has been navigated to...");

            //get parameters
            navParentName = NavigationContext.QueryString[App.MagicParentTag];
            navSelectedName = NavigationContext.QueryString[App.MagicSelectedTag];
            navSelectedType = NavigationContext.QueryString[App.MagicTypeTag];

            Debug.WriteLine("magic pivot got a type = " + navSelectedType);

            setupBinding();
        }
    }

    public class MagicPivotViewModel
    {
        /*
         * what goes inside each column in the collection of them
         */
        public class ColumnContent
        {
            //FIXME need to notify changed?
            public string ColumnHeader {get; set;}
            public ObservableCollection<string> ColContent {get; set;}

            private List<string> AllCategories;
            private List<string> AllTopics;

            public ColumnContent(Category c)
            {
                this.ColumnHeader = c.Name;

                ColContent = new ObservableCollection<string>();

                AllCategories = new List<string>();
                AllTopics = new List<string>();
            }

            /*
             * A strange hack that takes a string, figures out if its a category or topic, and returns a string
             * indicating which
             */
            public string findType(string q)
            {
                if (AllCategories.Contains(q))
                    return MagicPivot.MagicTypeCategory;
                else if (AllTopics.Contains(q))
                    return MagicPivot.MagicTypeTopic;
                else
                    return "";
            }

            public void setColumnContent()
            {
                IQueryable<Category> query =
                    from cats in App.mDB.CategoriesTable
                    where cats.Name == ColumnHeader
                    select cats;

                //add all sub categories
                foreach (Category c in query.FirstOrDefault().Categories)
                {
                    Debug.WriteLine("col content " + c.Name);
                    ColContent.Add(c.Name);
                    AllCategories.Add(c.Name);
                }

                //add all topics
                foreach (Topic c in query.FirstOrDefault().Topics)
                {
                    Debug.WriteLine("col content topic " + c.Name);
                    ColContent.Add(c.Name);
                    AllTopics.Add(c.Name);
                }
            }
        }

        //column index
        public int TabIndex { get; set; }

        //collection of column content
        public ObservableCollection<ColumnContent> Columns { get; set; }

        //names
        //FIXME need to notify changed
        public string ParentName { get; set; }
        public string SelectedName { get; set; }

        public MagicPivotViewModel(string pName, string selName)
        {
            TabIndex = 0;
            Columns = new ObservableCollection<ColumnContent>();

            this.ParentName = pName;
            this.SelectedName = selName;

            Debug.WriteLine("Made a new MagicPivot View Model with parent = " + pName + " & cur = " + selName);

            //force all data to update
            UpdateData();
        }

        /*
         * Update the data and index
         */
        public void UpdateData()
        {
            //run a query to fill the list of column headers
            //FIXME need to notify changed
            IQueryable<Category> query =
                from cats in App.mDB.CategoriesTable
                where cats.Name == ParentName
                select cats;

            int index = 0;
            foreach (Category c in query.FirstOrDefault().Categories)
            {
                Debug.WriteLine("Got Cat " + c.Name);

                ColumnContent cc = new ColumnContent(c);
                this.Columns.Add(cc);
                cc.setColumnContent();

                if (c.Name == SelectedName)
                {
                    TabIndex = index;
                }
                index++;
            }

            Debug.WriteLine("tab index = " + TabIndex);
        }
    }
    
}