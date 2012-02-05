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

namespace iFixit7
{
    public partial class MainPage : PhoneApplicationPage
    {
        public Panorama BigPanoGetter { get; private set; }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //now add to the pano
            PanoramaItem pi = null;
            for (int i = 0; i < 5; i++)
            {
                pi = new PanoramaItem();
                ListBox lb = new ListBox();
                pi.Content = lb;

                TextBlock tb = null;
                for (int j = 0; j < 10; j++)
                {
                    tb = new TextBlock();
                    tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);
                    tb.Text = "I am list item " + j + "!";
                    lb.Items.Add(tb);
                }

                pi.Header = "Biggish " + i;

                this.BigPano.Items.Add(pi);
            }
        }

        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as TextBlock).Text;
            //stash where we are about to navigate to...
            App.setNextArea("bobby");

            NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + s, UriKind.Relative));
            //make a xaml which we populate 
        }
    }
}