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

namespace iFixit7
{
    public partial class DeviceInfo : PhoneApplicationPage
    {
        //FIXME if we implement the property changing stuff here, the do we need to do the funny assignments?
        public TopicInfoViewModel infoVM = null;

        private string navTopicName;

        public DeviceInfo()
        {
            InitializeComponent();

            this.InfoStack.DataContext = infoVM;
            this.GuidesStack.DataContext = infoVM;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            //as far as I can think, this could only mean going back to the previous menus
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //get the device name that was passed and stash it
            this.navTopicName = this.NavigationContext.QueryString["Topic"];
            infoVM = new TopicInfoViewModel(navTopicName);

            Debug.WriteLine("Showing device info for [" + this.navTopicName + "]");

            InfoPano.Title = navTopicName;

            //API call to get the entire contents of the device info and populate it it returns (it calls populateUI
            //on its own when the operation completes
            new JSONInterface2().populateDeviceInfo(navTopicName, insertDevInfoIntoDB);
        }

        /*
         * Insert the data from the JSON parser into the database
         */
        private bool insertDevInfoIntoDB(DeviceInfoHolder devInfo)
        {
            Topic top = null;
            Debug.WriteLine("putting device info into DB...");

            //try to get a topic of this name from the DB
            //if it fails, make a new one. if it works, update the old
            top = App.mDB.TopicsTable.SingleOrDefault(t => t.Name == devInfo.topic_info.name);
            if (top == null)
            {
                top = new Topic();
            }
            else
            {
                //nuke the existing one so we dont collide on insert
                App.mDB.TopicsTable.DeleteOnSubmit(top);
                App.mDB.SubmitChanges();
            }

            Topic newTop = new Topic();
            //translate devInfo in a Topic()
            //name is already the same
            newTop.Name = devInfo.topic_info.name;
            newTop.Description = devInfo.description;
            newTop.ImageURL = devInfo.image.text + ".medium";       //scales the image
            newTop.Populated = true;

            newTop.Description = devInfo.description;

            //now do the same for all attached guides
            foreach (DIGuides g in devInfo.guides)
            {
                Debug.WriteLine("\tguide " + g.title);

                //search if the guide already exists, and get or update it
                Guide gNew = new Guide();
                Guide gOld = null;
                gOld = App.mDB.GuidesTable.FirstOrDefault(other => other.Title == g.title);
                if (gOld != null)
                {
                    App.mDB.GuidesTable.DeleteOnSubmit(gOld);
                    App.mDB.SubmitChanges();
                
                    //transfer info
                    gNew.Title = gOld.Title;
                    gNew.Summary = gOld.Summary;
                    gNew.URL = gOld.URL;
                    gNew.GuideID = gOld.GuideID;
                    gNew.Thumbnail = gOld.Thumbnail;
                    gNew.TitleImage = gOld.TitleImage;
                }

                gNew.FillFieldsFromDeviceInfo(navTopicName, g);

                // hang it below the topic, it its collection of guides
                newTop.Guides.Add(gNew);

                //FIXME do we need to specifically add this to the guide table? is that magic?
            }

            //insert the Topic() into the database
            App.mDB.TopicsTable.InsertOnSubmit(newTop);
            App.mDB.SubmitChanges();

            //force the view model to update
            infoVM.UpdateData();

            //force the views to update
            this.InfoStack.DataContext = infoVM;
            this.GuidesStack.DataContext = infoVM;

            //disable the loading bars
            this.LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
            this.LoadingBarGuides.Visibility = System.Windows.Visibility.Collapsed;

            return true;
        }

        /*
         * This fires whenever an item in the list of guides is tapped. Sender is a StackPanel, tag
         * is guide ID
         */
        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string guideTag = "";
            guideTag = (sender as StackPanel).Tag.ToString();
            Debug.WriteLine("device info tapped guide id = " + guideTag);
            NavigationService.Navigate(new Uri("/Guide.xaml?GuideID=" + guideTag, UriKind.Relative));
        }

        /*
         * These two are the handlers for the application bar buttons
         */
        private void AppBarSearch_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SearchView.xaml", UriKind.Relative));
        }
        private void AppBarFavorites_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FavoriteItems.xaml", UriKind.Relative));
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
        public String Description { get; set; }

        //this is the list of guides
        public ObservableCollection<Guide> GuideList { get; set; }

        public TopicInfoViewModel(string name)
        {
            this.Name = name;
            ImageURL = "";
            Description = "";
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
            top = App.mDB.TopicsTable.SingleOrDefault(t => t.Name == Name);
            if (top == null)
            {
                return;
            }

            //fill in internal collections
            this.Name = top.Name;
            this.Description = top.Description;
            this.ImageURL = top.ImageURL;

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