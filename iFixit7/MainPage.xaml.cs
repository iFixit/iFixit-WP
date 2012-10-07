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

        //see http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh407286(v=vs.88).aspx
        //for data binding info

        //private iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString);

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //data binding is called elsewhere?
            //initDataBinding();

            //what about this? I think both are fair
            using (iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString))
            {
                /*
                //select the categories under the root node
                DataContext = from cats in dbHand.CategoriesTable
                              where cats.Name == "root"
                              select cats.Categories;
                */

                //for testing. Should get all categories ever
                DataContext = from cats in dbHand.CategoriesTable
                              select cats;
            }
        }

        public void initDataBinding(){
            //setup the data binding stuff
            using (iFixitDataContext dbHand = new iFixitDataContext(App.DBConnectionString))
            {
                this.CatagoryList.ItemsSource = from cats in dbHand.CategoriesTable
                                                select cats;
                /*
                this.CatagoryList.ItemsSource =
                    from cats in dbHand.CategoriesTable
                    where cats.Name == "root"
                    select cats.Categories;
                */

                /*
                //query for objects in root. This finds 'root', but the 1:M's are empty
                IQueryable<Category> query =
                    from cats in dbHand.CategoriesTable
                    where cats.Name == "root"
                    select cats;

                Debug.WriteLine("starting to print results");
                foreach (Category c in query)
                {
                    Debug.WriteLine(">" + c.Name);
                }

                Category upperCategories = query.FirstOrDefault();

                //set the returned list as the data source
                if (upperCategories != null)
                {
                    this.CatagoryList.ItemsSource = upperCategories.Categories;
                }
                */
            }
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

        // callAreasAPI
        /*
        public void MainPage_callAreasAPI(MainPage sender, Category tree)
        {
            Debug.WriteLine("we got a tree, right? PROCESS IT");

            App.setEnitreAreaHierarchy(tree);


            // THIS IS ALL WRONG! But I am doing it for now..... 
            PanoramaItem pi = null;
            ListBox lb = null;
            foreach (Category n in tree.Categories)
            {
                if(n.Categories != null) {
                    pi = new PanoramaItem();
                    pi.Header = n.Name;
                    lb = new ListBox();
                    pi.Content = lb;

                    ListBoxItem lbi = null;
                    TextBlock tb = null;
                    foreach (Category item in n.Categories)
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
*/
    }
}