﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using System.Diagnostics;

namespace iFixit7
{
    public partial class MagicPivot : PhoneApplicationPage
    {
        private MagicPivotViewModel vm = null;

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
            vm = new MagicPivotViewModel(navParentName, navSelectedName);

            //set data context to the view model
            this.DataContext = vm;
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
            State.Add("NavigatedToHeader", (this.SmartPivot.SelectedItem as MagicPivotViewModel.ColumnContent).ColumnHeader);

            //FIXME save current index in VM? alter params?
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

            //private List<string> AllCategories;
            //private List<string> AllTopics;
            private Dictionary<string, string> TypeRefs;

            public ColumnContent(Category c)
            {
                this.ColumnHeader = c.Name;

                ColContent = new ObservableCollection<string>();

                //AllCategories = new List<string>();
                //AllTopics = new List<string>();
                TypeRefs = new Dictionary<string, string>();
            }

            /*
             * A strange hack that takes a string, figures out if its a category or topic, and returns a string
             * indicating which
             */
            public string findType(string q)
            {
                //if (AllCategories.Contains(q))
                //    return MagicPivot.MagicTypeCategory;
                //else if (AllTopics.Contains(q))
                //    return MagicPivot.MagicTypeTopic;
                //else
                //    return "";
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
                        ColContent.Add(c.Name);
                        TypeRefs.Add(c.Name, MagicPivot.MagicTypeCategory);
                        //AllCategories.Add(c.Name);
                    }

                    //add all topics
                    foreach (Topic c in colCat.Topics)
                    {
                        //Debug.WriteLine("col content topic " + c.Name);
                        ColContent.Add(c.Name);
                        TypeRefs.Add(c.Name, MagicPivot.MagicTypeTopic);
                        //AllTopics.Add(c.Name);
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
            if (ParentName != "" && Columns.Count == 0)
                UpdateData();
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