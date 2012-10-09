using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Newtonsoft.Json.Linq;

using System.Text;
using System.IO;
using System.Net;

using System.Diagnostics;

namespace iFixit7
{
    public partial class MainPage : PhoneApplicationPage
    {
        public Panorama BigPanoGetter { get; private set; }

        //public delegate void AreaCallEventHandler(iFixitJSONHelper sender, List<Node> tree);
        //public static event AreaCallEventHandler callAreasAPI;

        //see http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh407286(v=vs.88).aspx
        //for data binding info

        private iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString);

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //data binding is called elsewhere?
            //initDataBinding();

            //what about this? I think both are fair
            /*
            using (iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString))
            {
                //select the categories under the root node
                DataContext = from cats in dbHand.CategoriesTable
                              where cats.Name == "root"
                              select cats.Categories;

                ////for testing. Should get all categories ever
                //IQueryable<Category> query = from cats in dbHand.CategoriesTable
                //              select cats;

                //this.CatagoryList.ItemsSource = query;
            }
        */
        }

        public void initDataBinding(){
            //setup the data binding stuff
            /*
            this.CatagoryList.ItemsSource = from cats in dbHand.CategoriesTable
                                            select cats;
            */
            /*
            this.CatagoryList.ItemsSource =
                from cats in dbHand.CategoriesTable
                where cats.Name == "root"
                select cats.Categories;
            */
            IQueryable<Category> query =
                from cats in App.mDB.CategoriesTable
                where cats.Name == "root"
                select cats;
            this.CatagoryList.ItemsSource = query.FirstOrDefault().Categories;

            /*
            //query for objects in root. This finds 'root', but the 1:M's are empty
            IQueryable<Category> query =
                from cats in dbHand.CategoriesTable
                where cats.Name == "root"
                select cats;

            Debug.WriteLine("starting to print results");
            foreach (Category c in query)
            {
                Debug.WriteLine(">" + c.Name);
            }

            Category upperCategories = query.FirstOrDefault();

            //set the returned list as the data source
            if (upperCategories != null)
            {
                this.CatagoryList.ItemsSource = upperCategories.Categories;
            }
            */
        }

        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as StackPanel).Tag as String;
            Debug.WriteLine("main page tapped > [" + s + "]");

            NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + App.RootCategoryName +
                "&SelectedCategory=" + s +
                "&SelectedType=" + "category",
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Clear selected index when navigated to
            this.CatagoryList.SelectedIndex = -1;
        }
    }
}