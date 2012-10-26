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
            string s = (sender as StackPanel).Tag as String;
            //Debug.WriteLine("main page tapped CACHED > [" + s + "]");
            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + s,
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //force an update of binding for the cached column
            IQueryable<Topic> queryCached =
                from top in App.mDB.TopicsTable
                where top.Populated == true
                select top;
            this.CachedList.ItemsSource = queryCached;
        }
    }
}