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
        }

        void tb_Tap(object sender, GestureEventArgs e)
        {
            //Node got = null;
            //int count = 0;
            //int set = 0;
            //bool flag = false;
            //string s = (sender as TextBlock).Text;
            ////stash where we are about to navigate to...
            //foreach (Group n in App.getEnitreAreaHierarchy().Groups)
            //{
            //    count = 0;
            //    foreach (Node brand in n.getChildrenList())
            //    {
            //        if (brand.getName().Equals(s))
            //        {
            //            got = n;
            //            set = count;
            //            flag = true;
            //        }

            //        if (!flag)
            //        count++;
            //        //Debug.WriteLine("looking at " + n.getName());
            //    }
                
            //}
            ////set a handle for where to navigate next
            //App.setNextArea(got, set);

            //NavigationService.Navigate(new Uri("/MagicPivot.xaml?page=" + s, UriKind.Relative));
        }

        public void getAreas()
        {
            iFixitJSONHelper ifj = new iFixitJSONHelper();

            Debug.WriteLine("about to get areas....");

            ifj.callAreasAPI += new iFixitJSONHelper.AreaCallEventHandler(MainPage_callAreasAPI);
            ifj.doAPICallAsync(iFixitJSONHelper.IFIXIT_API_AREAS);
        }

        // callAreasAPI
        public void MainPage_callAreasAPI(MainPage sender, Group tree)
        {
            Debug.WriteLine("we got a tree, right? PROCESS IT");

            App.setEnitreAreaHierarchy(tree);


            /* THIS IS ALL WRONG! But I am doing it for now..... */
            PanoramaItem pi = null;
            ListBox lb = null;
            foreach (Group n in tree.Groups)
            {
                if(n.Groups != null) {
                    pi = new PanoramaItem();
                    pi.Header = n.Name;
                    lb = new ListBox();
                    pi.Content = lb;

                    ListBoxItem lbi = null;
                    TextBlock tb = null;
                    foreach (Group item in n.Groups)
                    {
                        lbi = new ListBoxItem();
                        tb = new TextBlock();

                        tb.Text = item.Name;
                        tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);

                        lbi.Content = tb;
                        lb.Items.Add(lbi);
                    }
                    foreach (Device item in n.Devices)
                    {
                        lbi = new ListBoxItem();
                        tb = new TextBlock();

                        tb.Text = item.Name;
                        tb.Tap += new EventHandler<GestureEventArgs>(tb_Tap);

                        lbi.Content = tb;
                        lb.Items.Add(lbi);
                    }

                    this.BigPano.Items.Add(pi);
                }
            }

            Debug.WriteLine("finished adding to big pano");
        }
    }
}