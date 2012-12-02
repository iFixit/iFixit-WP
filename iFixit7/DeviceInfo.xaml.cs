using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;

namespace iFixit7
{
    public partial class DeviceInfo : PhoneApplicationPage
    {
        public static string NETWORK_FLAG = "OfflineCached";

        //FIXME if we implement the property changing stuff here, the do we need to do the funny assignments?
        public TopicInfoViewModel infoVM = null;

        private string navTopicName;
        private bool InOfflineMode = false;

        //used to prevent scrolling in browser
        private WebBrowserHelper browserHelper;

        public DeviceInfo()
        {
            InitializeComponent();

            this.InfoStack.DataContext = infoVM;
            this.GuidesStack.DataContext = infoVM;

            browserHelper = new WebBrowserHelper(InfoBrowser);
            browserHelper.ScrollDisabled = true;
        }

        /*
         * This is what fires when a script in the browser raises an event
         */
        void InfoBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine("e.value == " + e.Value);

            int height = -1;

            browserHelper.ScrollDisabled = true;

            //if we got the resize info http://dan.clarke.name/2011/05/resizing-wp7-webbrowser-height-to-fit-content/
            if (int.TryParse(e.Value, out height))
            {
                Debug.WriteLine("\tsetting browser height to " + height);
                double newHeight = height * 1.5;

                InfoBrowser.Height = newHeight;
            }
            //if we got a URL to navigate to
            else if (Uri.IsWellFormedUriString(e.Value, UriKind.RelativeOrAbsolute))
            {
                Debug.WriteLine("\tnavigating to e.value");
                WebBrowserTask webBrowserTask = new WebBrowserTask { Uri = new Uri(e.Value) };
                webBrowserTask.Show();
            }
            
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            //as far as I can think, this could only mean going back to the previous menus
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.State[InfoPano.Title.ToString()] = this.InfoPano.SelectedIndex;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //if there is already a VM, dont build a new one
            if (infoVM != null)
                return;

            //get the device name that was passed and stash it
            this.navTopicName = this.NavigationContext.QueryString["Topic"];

            //figure out if it was sent while offline
            if (this.NavigationContext.QueryString.ContainsKey(DeviceInfo.NETWORK_FLAG))
            {
                InOfflineMode = true;
            }

            //build a new view model. IMMEDIATELY runs a DB query to fill itself
            infoVM = new TopicInfoViewModel(navTopicName);

            Debug.WriteLine("Showing device info for [" + this.navTopicName + "]");

            InfoPano.Title = navTopicName;

            string key = InfoPano.Title.ToString();
            bool resuming = false;
            if (this.State.ContainsKey(key))
            {
                resuming = true;
            }

            //API call to get the entire contents of the device info and populate it it returns (it calls populateUI
            //on its own when the operation completes  && !resuming
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                new JSONInterface2().populateDeviceInfo(navTopicName, insertDevInfoIntoDB);
            }
            else
            {
                updateInfoBrowser();

                //force the views to update
                this.InfoStack.DataContext = infoVM;
                this.GuidesStack.DataContext = infoVM;

                //disable the loading bars
                this.LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
                this.LoadingBarGuides.Visibility = System.Windows.Visibility.Collapsed;
            }

            // if there is a saved index, re-navigate to it
            if(resuming)
            {
                int selectedTabIndex = (int) this.State[key];
                if (0 <= selectedTabIndex && selectedTabIndex < InfoPano.Items.Count)
                {
                    var t = InfoPano.Items[selectedTabIndex];
                    InfoPano.SetValue(Panorama.SelectedItemProperty, InfoPano.Items[selectedTabIndex]);
                }
            }
        }

        /*
         * Insert the data from the JSON parser into the database
         */
        private bool insertDevInfoIntoDB(DeviceInfoHolder devInfo)
        {
            //something is wrong and the device was not found. Bail
            if (devInfo == null)
            {
                Debug.WriteLine("something went terribly wrong with a DeviceInfo. Bailing");
                NavigationService.GoBack();
                return false;
            }

            Topic top = null;
            Debug.WriteLine("putting device info into DB...");

            //try to get a topic of this name from the DB
            //if it fails, make a new one. if it works, update the old
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                bool gotTopicFromDB = true;
                top = DBHelpers.GetCompleteTopic(devInfo.topic_info.name, db);

                if (top == null)
                {
                    top = new Topic();
                    gotTopicFromDB = false;
                }

                //translate devInfo in a Topic()
                //name is already the same
                top.Name = devInfo.topic_info.name;
                top.parentName = devInfo.title;
                top.Contents = devInfo.description;
                top.ImageURL = devInfo.image.text + ".medium";
                top.Populated = true;

                //TODO inject metatdata here like # answers
                top.Description = "";
                top.Description += "<h2>" + devInfo.guides.Length + " Guides</h2>";
                top.Description += "<h2>" + devInfo.solutions.count + " Solutions</h2>";
                top.Description += prepHTML(devInfo.contents);

                //now do the same for all attached guides
                foreach (DIGuides g in devInfo.guides)
                {
                    Debug.WriteLine("\tguide " + g.title);

                    //search if the guide already exists, and get or update it
                    Guide gOld = null;
                    gOld = db.GuidesTable.FirstOrDefault(other => other.Title == g.title);
                    if (gOld == null)
                    {
                        gOld = new Guide();
                        db.GuidesTable.InsertOnSubmit(gOld);
                    }

                    gOld.FillFieldsFromDeviceInfo(navTopicName, g);

                    // hang it below the topic, it its collection of guides
                    top.AddGuide(gOld);

                    //FIXME do we need to specifically add this to the guide table? is that magic?
                    db.SubmitChanges();
                }

                if (!gotTopicFromDB)
                {
                    db.TopicsTable.InsertOnSubmit(top);
                }

                //update the Topic() into the database
                db.SubmitChanges();

                //force the view model to update
                infoVM.UpdateData();
                updateInfoBrowser();

                //force the views to update
                this.InfoStack.DataContext = infoVM;
                this.GuidesStack.DataContext = infoVM;

                //disable the loading bars
                this.LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
                this.LoadingBarGuides.Visibility = System.Windows.Visibility.Collapsed;

            }

            return true;
        }

        /*
         * This fires whenever an item in the list of guides is tapped. Sender is a StackPanel, tag
         * is guide ID
         */
        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string guideTag = "";
            guideTag = (sender as Grid).Tag.ToString();
            double opacity = (sender as Grid).Opacity;

            Debug.WriteLine("device info tapped guide id = " + guideTag);

            //figure out if the guide can be navigated to
            //if online or opacity is 1 (it has been cached), can navigate there
            if (!InOfflineMode || opacity == 1.0)
            {
                NavigationService.Navigate(new Uri("/Guide.xaml?GuideID=" + guideTag, UriKind.Relative));
            }
            else
            {
                MessageBox.Show("This guide has not been cached for offline viewing.");
            }
        }


        private void updateInfoBrowser()
        {
            SetBrowserBackground();
            InfoBrowser.NavigateToString(infoVM.Description);

            //keep it hidden until content is visible
            InfoBrowser.Visibility = System.Windows.Visibility.Visible;
        }

        /*
         * adds needed stuff to the HTML to be displayed in the browser
         */
        private string prepHTML(string baseHTML)
        {
            string o = "";
            o += "<html><head>";

            //prevent zooming
            o += "<meta name='viewport' content='width=320,user-scalable=no'/>";

            //inject the theme
            o += "<style type='text/css'>" +
                "body {{font-size:1.0em;background-color:" + FetchBackgroundColor() + ";" +
                "color:" + FetchFontColor() + ";}} " + "</style>";

            //inject the script to pass link taps out of the browser
            o += "<script type='text/javascript'>";
            o += @"function getLinks(){ 
                a = document.getElementsByTagName('a');
                    for(var i=0; i < a.length; i++){
                    var msg = a[i].href;
                    a[i].onclick = function() {notify(msg);
                    };
                    }
                    }
                    function notify(msg) {
                    window.external.Notify(msg);
                    event.returnValue=false;
                    return false;
                }";

            //inject the script to find height
            o += @"function Scroll() {
                            var elem = document.getElementById('content');
                            window.external.Notify(elem.scrollHeight + '');
                        }
                    ";

            //remove all anchors
            while (baseHTML.Contains("<a class=\"anchor\""))
            {
                int start = baseHTML.IndexOf("<a class=\"anchor\"");
                int end = baseHTML.IndexOf("</h2>", start);

                baseHTML = baseHTML.Remove(start, end - start);
            }

            //FIXME remove this when we fix the webbrowser
            //remove all links
            while (baseHTML.Contains("<a href"))
            {
                //remove most of the link
                int start = baseHTML.IndexOf("<a href");
                int end = baseHTML.IndexOf(">", start);

                baseHTML = baseHTML.Remove(start, end + 1 - start);

                //remove end tag
                start = baseHTML.IndexOf("</a>", start);
                baseHTML = baseHTML.Remove(start, "</a>".Length);
            }

            o += @"window.onload = function() {
                    Scroll();
                    getLinks();
                }";

            o += "</script>";
            o += "</head>";
            o += "<body><div id='content'>";
            //o += "<img src='" + infoVM.ImageURL + "'>";
            o += baseHTML.Trim();
            o += "</div></body>";
            o += "</html>";
            return o;
        }
        private string FetchBackgroundColor()
        {
            return IsBackgroundBlack() ? "#000;" : "#fff";
        }

        private string FetchFontColor()
        {
            return IsBackgroundBlack() ? "#fff;" : "#000";
        }

        private bool IsBackgroundBlack()
        {
            return ((Visibility)Application.Current
                .Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);
        }

        private void SetBrowserBackground()
        {
            InfoBrowser.Background =
              (Brush)Application.Current.Resources["PhoneBackgroundBrush"];
        }

        /*
         * These two are the handlers for the application bar buttons
         */
        private void AppBarSearch_Click(object sender, EventArgs e)
        {
            // error out and go back if there is no netowork connection
            if (!DeviceNetworkInformation.IsNetworkAvailable)
            {
                MessageBox.Show("Search cannot be used without an internet connection.");
                return;
            }
            NavigationService.Navigate(new Uri("/SearchView.xaml", UriKind.Relative));
        }
        private void AppBarFavorites_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FavoriteItems.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            WebBrowserTask wbt = new WebBrowserTask();
            wbt.Uri = new Uri(infoVM.BaseURL, UriKind.Absolute);
            wbt.Show();
        }
    }

    /*
     * The view model for the info column and guides
     */
    //FIXME needs to notify when its members get updated
    //FIXME this can almost certainly be paired down to nearly nothing (can we refer directly to the Topic that the query gets?)
    public class TopicInfoViewModel
    {
        public String Name { get; set; }
        public String ImageURL { get; set; }
        public String Contents { get; set; }
        public String Description { get; set; }
        public String BaseURL { get; set; }

        //this is the list of guides
        public ObservableCollection<Guide> GuideList { get; set; }

        public TopicInfoViewModel(string name)
        {
            this.Name = name;
            ImageURL = "";
            Description = "";
            Contents = "";
            BaseURL = "";
            GuideList = new ObservableCollection<Guide>();

            UpdateData();
        }

        /*
         * force this view model to update all its internal data
         */
        public void UpdateData()
        {
            Debug.WriteLine("Updating view model...");

            //run queries
            Topic top = null;
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                top = DBHelpers.GetCompleteTopic(Name, db);
            }
            if (top == null)
            {
                Debug.WriteLine(">>couldnt find DeviceInfo topic in DB to display it");
                return;
            }

            //fill in internal collections
            this.Name = top.Name;
            this.Description = top.Description;
            this.Contents = top.Contents;
            this.ImageURL = top.ImageURL;

            this.BaseURL = "http://www.ifixit.com/Device/" + this.Name;

            //fill in guides
            GuideList.Clear();
            foreach (Guide g in top.Guides)
            {
                Debug.WriteLine("\tinside updating view model. Found guide: " + g.Title);
                GuideList.Add(g);
            }
        }

    }
}