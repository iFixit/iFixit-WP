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
        public TopicInfoViewModel infoVM = null;

        private string navTopicName;

        public DeviceInfo()
        {
            InitializeComponent();

            //IQueryable<Topic> query =
            //    from tops in App.mDB.TopicsTable
            //    select tops;
            //foreach (Topic t in query)
            //{
            //    Debug.WriteLine("topic: " + t.Name);
            //}

            //this.DataContext = vm;
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

            Debug.WriteLine("Showing device info for [" + navTopicName + "]");

            InfoPano.Title = navTopicName;

            //API call to get the entire contents of the device info and populate it it returns (it calls populateUI
            //on its own when the operation completes
            JSONInterface2 ji = new JSONInterface2();
            ji.populateDeviceInfo(navTopicName, insertDevInfoIntoDB);
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
            newTop.ImageURL = devInfo.image.text;
            newTop.Populated = true;

            newTop.Description = devInfo.description;
            //scale the image
            newTop.ImageURL = devInfo.image.text + ".medium";

            //now do the same for all attached guides
            foreach (DIGuides g in devInfo.guides)
            {
                Debug.WriteLine("\tguide " + g.title);

                //search if the guide already exists, and get or update it
                Guide gNew = new Guide();
                Guide gOld = null;
                gOld = App.mDB.GuidesTable.SingleOrDefault(other => other.Title == g.title);
                if (gOld != null)
                {
                    App.mDB.GuidesTable.DeleteOnSubmit(gOld);
                    App.mDB.SubmitChanges();
                
                    //transfer info
                    gNew.Title = gOld.Title;
                    gNew.Subject = gOld.Subject;
                    gNew.URL = gOld.URL;
                    gNew.GuideID = gOld.GuideID;
                    gNew.Thumbnail = gOld.Thumbnail;
                }

                gNew.Title = g.title;
                gNew.Subject = g.subject;
                gNew.URL = g.url;
                gNew.GuideID = g.guideid;
                gNew.Thumbnail = g.thumbnail;

                // hang it below the topic, it its collection of guides
                newTop.Guides.Add(gNew);

                //FIXME do we need to specifically add this to the guide table? is that magic?
            }

            //insert the Topic() into the database
            App.mDB.TopicsTable.InsertOnSubmit(newTop);
            //FIXME explodes here (just closes...) If we break and navigate inside with the debugger and try to examine an
            //individual guide, it explodes. cannot evaluate expression ID
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
        private void StackPanel_Tap(object sender, GestureEventArgs e)
        {

        }
    }

    /*
     * The view model for the info column and guides
     */
    //FIXME needs to notify when its members get updated!!
    public class TopicInfoViewModel
    {
        /*
         * The internal class which holds the content of each row of the guide boxes
         */
        public class GuidePreview
        {
            public String Name { get; set; }

            public GuidePreview(string n)
            {
                this.Name = n;
            }
        }

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

            //FIXME HACK remove
            //ImageURL = "http://www.ifixit.com/igi/lBRuNjQShvBxWol6.thumbnail";

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
            //try to get a topic of this name from the DB. If it fails, do nothing
            //IQueryable<Topic> query =
            //    from tops in App.mDB.TopicsTable
            //    where tops.Name == this.Name
            //    select tops;
            //top = query.FirstOrDefault();
            //Debug.WriteLine("Found some querties: " + query.Count());
            top = App.mDB.TopicsTable.SingleOrDefault(t => t.Name == Name);
            Debug.WriteLine("\tquery returned [" + top + "] for name = " + this.Name);
            Debug.WriteLine("\t top: " + top.Name + ", " + top.ImageURL);
            if (top == null)
            {
                return;
            }

            //fill in internal collections
            this.Name = top.Name;
            this.Description = top.Description;
            this.ImageURL = top.ImageURL;

            //fill in guides
            foreach (Guide g in top.Guides)
            {
                Debug.WriteLine("\tinside updating view model. Found guide: " + g.Title);
                GuideList.Add(g);
            }
        }

    }

    /*
     * A class to represent a single line describing a guide.
     * Type [guide, teardown], title, thumbnail, subject[light sensor]
     */
    class GuideEntry
    {
        string title;
        Uri thumb;
        string subject;
        string type;
        int guideID;

        public GuideEntry(DIGuides dg)
        {
            title = dg.title;
            //these are already .thumbnails
            thumb = new Uri(dg.thumbnail);
            subject = dg.subject;
            type = dg.type;
            guideID = int.Parse(dg.guideid);
        }
        public GuideEntry(string itit, string imgUrl, string isub, string itype, int gid)
        {
            title = itit;
            thumb = new Uri(imgUrl);
            subject = isub;
            type = itype;
            guideID = gid;
        }

        /*
         * Returns a ListBoxEntry with all the stuff needed to display this row of data
         */
        public Grid getRow()
        {
            Grid g = new Grid();

            //add 3 cols
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            //g.ColumnDefinitions.Add(new ColumnDefinition());
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            //g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

            //add 2 rows
            //g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            //g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100) });
            //g.RowDefinitions.Add(new RowDefinition());
            //g.RowDefinitions.Add(new RowDefinition{ Height = GridLength.Auto});
            //g.RowDefinitions.Add(new RowDefinition{ Height = GridLength.Auto});
            g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            //g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            //add the title
            TextBlock tbTitle = new TextBlock();
            //tbTitle.MaxWidth = 480 - 30;
            tbTitle.Text = this.title;
            tbTitle.TextWrapping = TextWrapping.Wrap;
            tbTitle.FontWeight = FontWeights.Bold;
            //tbTitle.Padding = new Thickness(0, 5, 0, 9);
            Grid.SetColumn(tbTitle, 0);
            Grid.SetColumnSpan(tbTitle, 2);
            Grid.SetRow(tbTitle, 0);
            g.Children.Add(tbTitle);

            //add the subject
            TextBlock tbSubject = new TextBlock();
            //tbSubject.MaxWidth = 480 - 30;
            tbSubject.TextWrapping = TextWrapping.Wrap;
            tbSubject.Text = this.subject;
            //tbSubject.Padding = new Thickness(0, 5, 0, 9);
            Grid.SetColumn(tbSubject, 0);
            Grid.SetRow(tbSubject, 1);
            g.Children.Add(tbSubject);

            //add the title
            TextBlock tbType = new TextBlock();
            //tbType.MaxWidth = 480 - 30;
            //tbType.TextWrapping = TextWrapping.Wrap;
            tbType.Text = this.type;
            //tbType.Padding = new Thickness(0, 5, 0, 9);
            Grid.SetColumn(tbType, 1);
            Grid.SetRow(tbType, 1);
            g.Children.Add(tbType);

            //add the image
            Image i = new Image();
            //This does the actual image fetch
            i.Source = new BitmapImage(thumb);
            Grid.SetColumn(i, 2);
            Grid.SetRow(i, 0);
            Grid.SetRowSpan(i, 2);
            g.Children.Add(i);

            //and add the handler to make it navigate to a particular guide when tapped
            g.Tap += delegate(object sender, GestureEventArgs e)
            {
                Debug.WriteLine("navigating to guide " + guideID + " title = " + title);
                (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/Guide.xaml?guideID=" + guideID + "&guideTitle=" + title + "", UriKind.Relative));
            };

            //g.Background = new SolidColorBrush(Colors.DarkGray);

            return g;
        }
    }
}