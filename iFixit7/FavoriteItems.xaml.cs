using System;
using System.Collections.Generic;
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
using Microsoft.Phone.Net.NetworkInformation;

namespace iFixit7
{
    public partial class FavoriteItems : PhoneApplicationPage
    {
        public FavoriteItems()
        {
            InitializeComponent();
        }

        /*
         * Fires when a cached entry is tapped
         */
        private void Cached_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string toAppend = "";

            string s = (sender as StackPanel).Tag as String;

            //if we are offline, send a flag along with the navigation
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                toAppend = "&" + DeviceInfo.NETWORK_FLAG + "=true";
            }

            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + s + toAppend,
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //remove main page from back stack if offline
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                NavigationService.RemoveBackEntry();
            }

            //force an update of binding for the cached column
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                // this.CachedList.ItemsSource = DBHelpers.GetCompleteCategory("root", db).Categories;

                IQueryable<Topic> queryCached =
                    from top in db.TopicsTable
                    where top.Populated == true
                    select top;
                this.CachedList.ItemsSource = queryCached;
            }
        }
    }
}