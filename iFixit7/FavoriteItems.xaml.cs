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
using System.IO.IsolatedStorage;

namespace iFixit7
{
    public partial class FavoriteItems : PhoneApplicationPage
    {
        public FavoriteItems()
        {
            InitializeComponent();
        }

        /*
         * Fires when a cached entry is tapped
         */
        private void Cached_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string toAppend = "";

            string s = (sender as Grid).Tag as String;

            //if we are offline, send a flag along with the navigation
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                toAppend = "&" + DeviceInfo.NETWORK_FLAG + "=true";
            }

            NavigationService.Navigate(new Uri("/DeviceInfo.xaml?Topic=" + s + toAppend,
                UriKind.Relative));
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //remove main page from back stack if offline
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                NavigationService.RemoveBackEntry();
            }

            //force an update of binding for the cached column
            UpdateBinding();
        }
        private void UpdateBinding()
        {
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                // this.CachedList.ItemsSource = DBHelpers.GetCompleteCategory("root", db).Categories;

                IQueryable<Topic> queryCached =
                    from top in db.TopicsTable
                    where top.Populated == true
                    select top;
                this.CachedList.ItemsSource = queryCached;
            }
        }
        private void DeleteCached_Tap(object sender, EventArgs e)
        {
            //go through all guides/steps/lines and topics and dump them
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                var cachedGuides =
                    from g in db.GuidesTable
                    where g.Populated == true
                    select g;
                foreach (Guide g in cachedGuides)
                {
                    var cachedSteps =
                        from s in db.StepsTable
                        where s.parentName == g.GuideID
                        select s;

                    foreach (Step s in cachedSteps)
                    {
                        var cachedLines =
                            from l in db.LinesTable
                            where l.parentName == g.GuideID
                            select l;

                        //Debug.WriteLine("got " + cachedLines.Count() + " lines");
                        db.LinesTable.DeleteAllOnSubmit(cachedLines);
                    }
                    //Debug.WriteLine("got " + cachedSteps.Count() + " steps");
                    db.StepsTable.DeleteAllOnSubmit(cachedSteps);
                }
                //Debug.WriteLine("got " + cachedGuides.Count() + " guides");
                db.GuidesTable.DeleteAllOnSubmit(cachedGuides);

                var cachedTopics =
                        from top in db.TopicsTable
                        where top.Populated == true
                        select top;
                db.TopicsTable.DeleteAllOnSubmit(cachedTopics);

                db.SubmitChanges();
            }

            //nuke all cached images
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //iterate through all files
                if (isoStore.DirectoryExists(ImgCache.BASE_PATH))
                {
                    foreach (string file in isoStore.GetFileNames(ImgCache.BASE_PATH + "\\*"))
                    {
                        //Debug.WriteLine(file);
                        isoStore.DeleteFile(file);
                    }
                }
            }

            UpdateBinding();
        }
    }
}