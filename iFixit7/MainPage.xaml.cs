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

        // Constructor
        public MainPage()
        {

            //List<Node> myTree = iFixitJSONHelper.getAreas();
            InitializeComponent();

            //now add to the pano
            PanoramaItem pi = null;
            StackPanel sp = new StackPanel();
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

                //BigPano.Items.Add(pi);
                this.BigPano.Items.Add(pi);
            }
        }

        void lb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //cast this to get the spected item (sender as ListBox).SelectedItem
            NavigationService.Navigate(new Uri("the file.xaml", UriKind.Relative)); 
            //make a xaml which we populate

            throw new NotImplementedException();
        }
    }
}