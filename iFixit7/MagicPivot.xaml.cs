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

        private static int number;

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
                //lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);

                //now add items to this list of content
                TextBlock tb = null;
                for (int j = 0; j < 25; j++)
                {
                    tb = new TextBlock();
                    tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);
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

        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as TextBlock).Text;
            //stash where we are about to navigate to...
            App.setNextArea("bobby");

            //stash where we are about to navigate to?
            App.setNextArea("");

            Debug.WriteLine("A MagicPivot is about to navigate....");

            //figure out if it is a product (needs list of guides), individual guide, or another catagory. If catagory, call Magic. Else, call Guide
            
            //NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + number++, UriKind.Relative));

            NavigationService.Navigate(new Uri("/Guide.xaml?device=" + "iPhone+3G", UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A MagicPivot has been navigated to...");
        }
    }
}