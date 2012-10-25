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
using System.Windows.Media.Imaging;

namespace iFixit7
{
    public partial class MainPage : PhoneApplicationPage
    {
        public Panorama BigPanoGetter { get; private set; }

        private iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString);

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //force hub tiles to not animate
            HubTileService.FreezeGroup("AllTiles");
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

            //clear the loading bars when we are done loading data
            StopLoadingIndication(false);

            //force hub tiles to not animate
            HubTileService.FreezeGroup("AllTiles");
        }

        /*
         * A hook to disable the loading indicators and indicate no connectivity
         */
        public void StopLoadingIndication(Boolean failure)
        {
            this.LoadingBar.Visibility = System.Windows.Visibility.Collapsed;

            //replace the view with a view indicating failure
            if (failure)
            {
                StackPanel sp = new StackPanel();

                Image i = new Image();
                i.Source = new BitmapImage(new Uri("fist.png", UriKind.Relative));
                i.Stretch = Stretch.UniformToFill;
                i.HorizontalAlignment = HorizontalAlignment.Center;
                i.VerticalAlignment = VerticalAlignment.Center;

                TextBlock tb = new TextBlock();
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Foreground = new SolidColorBrush(Colors.White);
                tb.TextAlignment = TextAlignment.Center;
                tb.FontSize = 45;
                tb.Text = "Please connect to the internet and restart the app.";

                sp.Children.Add(i);
                sp.Children.Add(tb);
                App.Current.RootVisual = sp;
            }
        }

        /*
         * Fires when something in the live list is tapped
         */
        void tb_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string s = (sender as Grid).Tag as String;
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

            //force an update
            initDataBinding();
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
}