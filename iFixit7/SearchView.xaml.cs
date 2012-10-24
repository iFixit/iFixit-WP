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
    public partial class SearchView : PhoneApplicationPage
    {
        public SearchView()
        {
            InitializeComponent();
        }

        private void SearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchProgressStack.Visibility = System.Windows.Visibility.Visible;

            string searchQuery = SearchQueryTB.Text;

            // kick off async search query
            /*
             * Cleanup the view and display the results of a search. Called async.
             * Return value is meaningless
             */
            Search.SearchForString(searchQuery, delegate(List<SRResult> SearchResults)
            {
                //this allows the async web thread to do UI things on the UI thread
                Dispatcher.BeginInvoke(() =>
                {
                    //hide progress indicator
                    SearchProgressStack.Visibility = System.Windows.Visibility.Collapsed;

                    //display the items
                    SearchResultList.ItemsSource = SearchResults;
                });

                //return from the lambda
                return 0;
            });
        }

        /*
        * Fires when a search result is tapped
        */
        private void Search_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string name = (sender as TextBlock).Text as String;
            Debug.WriteLine("main page tapped SEARCH topic name > [" + name + "]");

            //FIXME can we tap on device pages as well as guides? We need to be able to handle that...
            //maybe a single char at the start of the tag? build that string in the SearchResult object
            //for now, can only tap on topics (which lead to device info)
            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + name, UriKind.Relative));
        }
    }
}