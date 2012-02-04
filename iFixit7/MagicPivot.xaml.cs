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
    public partial class MagicPivot : PhoneApplicationPage
    {
        private Object areaShown = null;

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");

            //get the area we are about to navigate to, so we can build the view from it
            areaShown = App.getNextArea();

            //now iterate across the tree we got, and build a view from it
            PivotItem pi = null;
            ListBox lb = null;
            for (int i = 0; i < 4; i++)
            {
                //build a pivot item for each larger catagory
                pi = new PivotItem();
                pi.Header = "PItem " + i;

                //add a list of content to that header
                lb = new ListBox();
                pi.Content = lb;

                //add a handler for the list box, to handle clicks
                lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);

                //now add items to this list of content
                TextBlock tb = null;
                for (int j = 0; j < 25; j++)
                {
                    tb = new TextBlock();
                    tb.Text = "I am some text, in a text block!";

                    lb.Items.Add(tb);
                }

                //add the entire thing to the pivot
                SmartPivot.Items.Add(pi);
            }

            /*
            PanoramaItem pi = null;
            for (int i = 0; i < 5; i++)
            {
                pi = new PanoramaItem();
                ListBox lb = new ListBox();
                lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);
                pi.Content = lb;

                TextBlock tb = null;
                for (int j = 0; j < 10; j++)
                {
                    tb = new TextBlock();
                    tb.Text = "I am list item " + j + "!";
                    lb.Items.Add(tb);
                }
                
                pi.Header = "Biggish " + i;

                this.BigPano.Items.Add(pi);
            }
             */
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //stash where we are about to navigate to?
            App.setNextArea("");

            Debug.WriteLine("we are navigating....");
            //if this list box is an endpoint, then we need to navigate to a guide list, which is something else

            //else, navigate to deeper in this hierarchy
            NavigationService.Navigate(new Uri("/Redirector.xaml?TRASH=" + "123", UriKind.Relative));
            
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("We have been navigated to...");
        }
    }
}