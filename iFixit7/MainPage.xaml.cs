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

        //public delegate void AreaCallEventHandler(iFixitJSONHelper sender, List<Node> tree);
        //public static event AreaCallEventHandler callAreasAPI;

        // Constructor
        public MainPage()
        {

            //List<Node> myTree = iFixitJSONHelper.getAreas();
            InitializeComponent();

            //make the JSON request for areas, and fill in the UI in the handler...
            getAreas();

            //now add to the pano
            /*
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
             */
        }

        void tb_Tap(object sender, GestureEventArgs e)
        {
            string s = (sender as TextBlock).Text;
            //stash where we are about to navigate to...
            App.setNextArea(null);

            NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + s, UriKind.Relative));
            //make a xaml which we populate 
        }

        


        public void getAreas()
        {
            iFixitJSONHelper ifj = new iFixitJSONHelper();

            Debug.WriteLine("about to get areas....");

            ifj.callAreasAPI += new iFixitJSONHelper.AreaCallEventHandler(MainPage_callAreasAPI);
            ifj.doAPICallAsync(iFixitJSONHelper.IFIXIT_API_AREAS);
        }

        public void MainPage_callAreasAPI(MainPage sender, List<Node> tree)
        {
            Debug.WriteLine("we got a tree, right? PROCESS IT");

            App.setEnitreAreaHierarchy(tree);


            /* THIS IS ALL WRONG! But I am doing it for now..... */
            PanoramaItem pi = null;
            ListBox lb = null;
            foreach (Node n in tree)
            {
                if(n.getChildrenList() != null) {
                    //Debug.WriteLine(">>" + n.getName());
                    pi = new PanoramaItem();
                    pi.Header = n.getName();

                    pi.Content = lb;

                    ListBoxItem lbi = null;
                    TextBlock tb = null;
                    foreach (Node item in n.getChildrenList())
                    {
                        Debug.WriteLine("got a child of " + n.getName());

                        lbi = new ListBoxItem();
                        tb = new TextBlock();

                        tb.Text = item.getName();

                        lbi.Content = tb;
                    }

                    this.BigPano.Items.Add(pi);
                }
            }

            Debug.WriteLine("finished adding to big pano?");

            //now add to the pano
            /*
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
             */
        }





    }
}