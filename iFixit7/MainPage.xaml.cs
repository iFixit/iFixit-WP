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

            //data binding is called elsewhere
            //initDataBinding();

            Debug.WriteLine("new main page");
        }

        public void initDataBinding(){
            if (App.mDB == null)
                return;

            //setup the data binding stuff for live column
            IQueryable<Category> queryCats =
                from cats in App.mDB.CategoriesTable
                where cats.Name == "root"
                select cats;
            this.CatagoryList.ItemsSource = queryCats.FirstOrDefault().Categories;

            //and binding for the cached column
            IQueryable<Topic> queryCached =
                from top in App.mDB.TopicsTable
                where top.Populated == true
                select top;
            this.CachedList.ItemsSource = queryCached;

            //clear the loading bar when we are done loading data
            this.LoadingBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        /*
         * Fires when something in the live list is tapped
         */
        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as StackPanel).Tag as String;
            Debug.WriteLine("main page tapped > [" + s + "]");

            NavigationService.Navigate(new Uri("/MagicPivot.xaml?CategoryParent=" + App.RootCategoryName +
                "&SelectedCategory=" + s +
                "&SelectedType=" + "category",
                UriKind.Relative));
        }

        /*
         * Fires when a cached entry is tapped
         */
        private void Cached_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as TextBlock).Text as String;
            Debug.WriteLine("main page tapped CACHED > [" + s + "]");
            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + s,
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //Clear selected index when navigated to
            this.CatagoryList.SelectedIndex = -1;

            //force an update
            initDataBinding();
        }

        
    }
}