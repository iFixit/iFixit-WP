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
        private Node areaShown = null;
        private int col = 0;

        public MagicPivot()
        {
            InitializeComponent();

            Debug.WriteLine("starting a new magic pivot...");

            //get the area we are about to navigate to, so we can build the view from it
            areaShown = App.getNextArea();
            col = App.getCurrCol();

            Debug.WriteLine("node we are navigating from is " + areaShown.getName());

            //now iterate across the tree we got, and build a view from it
            PivotItem pi = null;
            ListBox lb = null;
            //for (int i = 0; i < 4; i++)

            foreach (Node n in areaShown.getChildrenList())
            {
                //build a pivot item for each larger catagory
                pi = new PivotItem();
                pi.Header = n.getName();

                //add a list of content to that header
                lb = new ListBox();
                pi.Content = lb;

                //add a handler for the list box, to handle clicks
                //lb.SelectionChanged += new SelectionChangedEventHandler(lb_SelectionChanged);

                //now add items to this list of content
                TextBlock tb = null;
                //for (int j = 0; j < 25; j++)
                if (n.getChildrenList() != null)
                {
                    foreach (Node model in n.getChildrenList())
                    {
                        tb = new TextBlock();

                        //add a specuial handler to respond to being tapped
                        //tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);
                        tb.Tap += delegate(object sender, GestureEventArgs e)
                        {
                            Debug.WriteLine("A MagicPivot is about to navigate....");

                            //what is this? works without it...
                            App.setNextArea(null, 0);

                            //FIXME ??
                            //figure out if it is a product (needs list of guides), individual guide, or another catagory. If catagory, call Magic. Else, call Guide
                            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?device=" + model.getName(), UriKind.Relative));
                        };

                        tb.Text = model.getName();

                        lb.Items.Add(tb);
                    }
                }

                //add the entire thing to the pivot
                SmartPivot.Items.Add(pi);
            }
            Loaded += delegate { SmartPivot.SelectedIndex = col;};
        }

        /*
        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as TextBlock).Text;
            //stash where we are about to navigate to...
            App.setNextArea(null, 0);

            Debug.WriteLine("A MagicPivot is about to navigate....");

            //figure out if it is a product (needs list of guides), individual guide, or another catagory. If catagory, call Magic. Else, call Guide
            
            //NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + number++, UriKind.Relative));

            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?device=" + "iPhone+3G", UriKind.Relative));
        }
         */

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A MagicPivot has been navigated to...");
        }
    }
}