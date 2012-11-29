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
    public partial class SearchView : PhoneApplicationPage
    {
        private Brush SelectedOriginalBack;
        private StackPanel Selected;

        public SearchView()
        {
            InitializeComponent();
        }

        private void SearchButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SearchProgressStack.Visibility = System.Windows.Visibility.Visible;

            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                //hide progress indicator
                SearchProgressStack.Visibility = System.Windows.Visibility.Collapsed;

                MessageBox.Show("Search cannot be used without an internet connection.");
                return;
            }

            string searchQuery = SearchQueryTB.Text;

            //hide the keyboard
            this.Focus();

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
            string name = (sender as StackPanel).Tag as String;

            //mark the tapped item as tapped using the accent color
            SelectedOriginalBack = (sender as StackPanel).Background;
            Selected = (sender as StackPanel);
            Selected.Background = App.Current.Resources["PhoneAccentBrush"] as Brush;


            Debug.WriteLine("main page tapped SEARCH topic name > [" + name + "]");

            //FIXME can we tap on device pages as well as guides? We need to be able to handle that...
            //maybe a single char at the start of the tag? build that string in the SearchResult object
            //for now, can only tap on topics (which lead to device info)
            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + name, UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (SearchResultList.Items.Count == 0)
                this.SearchQueryTB.Focus();
            else
            {
                //clear the selection marker
                Selected.Background = SelectedOriginalBack;
            }
        }

        /*
         * Keydown method figures out if enter was hit, and executes the search if it was
         */
        private void SearchQueryTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Tap(null, null);
            }
        }
    }
}