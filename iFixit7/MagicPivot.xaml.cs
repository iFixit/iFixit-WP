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

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");

            this.SmartPivot.Tap += new EventHandler<GestureEventArgs>(tb_Tap);

            #region dead
            /*
            //get the area we are about to navigate to, so we can build the view from it
            areaShown = App.getNextArea();
            col = App.getCurrCol();

            Debug.WriteLine("node we are navigating from is " + areaShown.Name);

            //now iterate across the tree we got, and build a view from it
            PivotItem pi = null;
            ListBox lb = null;
            //foreach (Node n in areaShown.getChildrenList())
            //for (int dex = 0; dex < areaShown.getChildrenList().Count; dex++)
            for (int dex = 0; dex < areaShown.Categories.Count; dex++)
            {
                //Node n = areaShown.getChildrenList().ElementAt(dex);
                Category n = areaShown.Categories.ElementAt(dex);
                //build a pivot item for each larger catagory
                pi = new PivotItem();
                pi.Header = n.Name;

                //add a list of content to that header
                lb = new ListBox();
                pi.Content = lb;

                //add a handler for the list box, to handle clicks
                //lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);

                //now add items to this list of content
                TextBlock tb = null;
                //for (int j = 0; j < 25; j++)
                if (n.Categories != null)
                {
                    //foreach (Node model in n.getChildrenList())
                    for(int dex2 = 0; dex2 < n.Categories.Count; dex2++)
                    {
                        Category model = n.Categories.ElementAt(dex2);
                        int curIndex = dex2;

                        tb = new TextBlock();

                        //add a specuial handler to respond to being tapped
                        //tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);
                        tb.Tap += delegate(object sender, GestureEventArgs e)
                        {
                            Debug.WriteLine("A MagicPivot is about to navigate, but to where?!");

                            Debug.WriteLine("what index to store? dex = " + dex + " and dex2 = " + dex2);

                            //FIXME finish this logic!
                            //figure out if it is a product (needs list of guides), individual guide, or another catagory. If catagory, call Magic. Else, call DeviceInfo
                            if (model.Categories != null)
                            {
                                //Debug.WriteLine("this node has " + model.getChildrenList().Count + " children. It is called " + model.getName() + " and we think it is no leaf. Its index = " + curIndex);

                                //set the next leaf to work off of
                                //App.setNextArea(model, 0);
                                App.setNextArea(n, curIndex);
                                NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + model.Name, UriKind.Relative));
                            }
                            else
                            {
                                //need to set some the index? if we change columns, we want the back button to return to the proper one...
                                //App.setNextArea(n, -1);
                                NavigationService.Navigate(new Uri("/DeviceInfo.xaml?device=" + model.Name, UriKind.Relative));
                            }
                        };

                        tb.Text = model.Name;

                        lb.Items.Add(tb);
                    }
                }

                //add the entire thing to the pivot
                SmartPivot.Items.Add(pi);
            }
            Loaded += delegate { SmartPivot.SelectedIndex = col;};
             * 
             * */
            #endregion
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
            string parent = vm.Columns[vm.TabIndex].ColumnHeader;

            //FIXME need to figure out what the type is!
            NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + parent +
                "&SelectedCategory=" + selected +
                "&SelectedType=" + "category",
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

            //if the type is a topic, immediately navigate to a device info page?
            Debug.WriteLine("magic pivot got a type = " + navSelectedType);

            setupBinding();

            /*
            //?
            //App.setNextArea(null, 0);
            //Node sel = null;
            Category sel = null;
            if (App.getNextArea() != null && App.getCurrCol() > -1)
            {
                if (App.getNextArea().Categories != null)
                {
                    //sel = App.getNextArea().getChildrenList().ElementAt(App.getCurrCol());
                    sel = App.getNextArea().Categories.ElementAt(App.getCurrCol());
                }
            }

            //if we are about to switch to a tab index with no children, then actually redirect to a device info
            if (sel != null)
            {
                if (sel.Topics.Count == 0)
                {
                    Debug.WriteLine("we were navigating to a leaf. deviceinfo it");
                    App.setNextArea(null, 0);
                    NavigationService.Navigate(new Uri("/deviceinfo.xaml?device=" + sel.Name, UriKind.Relative));
                }
            }
            else
            {
                //back one more
                NavigationService.GoBack();
            }
             * */
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

            public ColumnContent(Category c)
            {
                this.ColumnHeader = c.Name;

                ColContent = new ObservableCollection<string>();
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
                }

                //add all topics
                foreach (Topic c in query.FirstOrDefault().Topics)
                {
                    Debug.WriteLine("col content topic " + c.Name);
                    ColContent.Add(c.Name);
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