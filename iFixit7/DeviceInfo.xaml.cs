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
        public TopicInfoViewModel infoVM;

        private string navTopicName;

        public DeviceInfo()
        {
            InitializeComponent();

            infoVM = new TopicInfoViewModel(navTopicName);

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
        private bool insertDevInfoIntoDB(DeviceInfoHolder devInfo){
            Topic top = null;
            Debug.WriteLine("putting device info into DB...");

            //try to get a topic of this name from the DB
            //if it fails, make a new one. if it works, update the old
            top = App.mDB.TopicsTable.SingleOrDefault(t => t.Name == devInfo.title);
            if (top == null)
            {
                Debug.WriteLine("\tgenerated new topic");
                top = new Topic();
            }

            //translate devInfo in a Topic()
            //name is already the same
            top.Description = devInfo.description;
            top.ImageURL = devInfo.image.text;
            top.Populated = true;

            //now do the same for all attached guides
            foreach (DIGuides g in devInfo.guides)
            {
                //search if the guide already exists
                //add if not
            }

            //insert the Topic() into the database
            App.mDB.TopicsTable.InsertOnSubmit(top);
            App.mDB.SubmitChanges();

            Debug.WriteLine("\tinserted = " + top.Description + " imageurl = " + top.ImageURL);

            //force the view model to update
            infoVM.UpdateData();

            //disable the loading bar
            this.LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;

            return true;

            //this is especially irrelevant
            /*
            //Fill in the UI:
            //now generate 2 tabs. one with a list of guides, and one with a screen of info (like description, name, image, etc) about the device.
            //when the user selects a guide, naviate to a Guide.xaml with the guide ID, and it will fetch the guide and display it
            PivotItem piInfo = new PivotItem();
            PivotItem piGuides = new PivotItem();

            //reverse order to reverse order of tabs
            InfoPano.Items.Add(piGuides);
            InfoPano.Items.Add(piInfo);

            //now fill in the tabs, starting with info
            piInfo.Header = "Information";
            ListBox infoList = new ListBox();
            piInfo.Content = infoList;

            //add device image
            if (devInfo.image.text != null && devInfo.image.text != "")
            {
                Image infoImg = new Image();
                infoImg.Source = new BitmapImage(new Uri(devInfo.image.text + ".standard"));
                infoList.Items.Add(infoImg);
            }

            //add description
            TextBlock infoDesc = new TextBlock();
            infoDesc.MaxWidth = 480 - 30;
            infoDesc.TextWrapping = TextWrapping.Wrap;
            infoDesc.Text = devInfo.description;
            infoDesc.Padding = new Thickness(0, 5, 0, 9);
            infoList.Items.Add(infoDesc);

            //fill in guides
            piGuides.Header = "Guides";
            ListBox guideList = new ListBox();
            piGuides.Content = guideList;

            ListBoxItem lPad = null;
            GuideEntry ge = null;
            //iterate through all guides, adding each one
            foreach(DIGuides graw in devInfo.guides){
                ge = new GuideEntry(graw);
                guideList.Items.Add(ge.getRow());

                lPad = new ListBoxItem();
                lPad.Padding = new Thickness(5, 5, 5, 5);
                guideList.Items.Add(lPad);
            }

            return true;
             * */
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
        }

        public String Name { get; set; }
        public String ImageURL { get; set; }
        public String Description { get; set; }

        //this is the list of guides
        public ObservableCollection<GuidePreview> GuideList { get; set; }

        public TopicInfoViewModel(string name)
        {
            this.Name = name;
            ImageURL = "";
            Description = "";
            GuideList = new ObservableCollection<GuidePreview>();

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
            top = App.mDB.TopicsTable.SingleOrDefault(t => t.Name == Name);
            Debug.WriteLine("\tquery returned " + top);
            if (top == null)
            {
                return;
            }

            //fill in internal collections
            Debug.WriteLine("\tgot a topic of name " + Name + ", populating view model");
            this.Name = top.Name;
            this.Description = top.Description;
            this.ImageURL = top.ImageURL;

            Debug.WriteLine("\tdesc = " + this.Description + " imageurl = " + this.ImageURL);
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
            g.RowDefinitions.Add(new RowDefinition{ Height = GridLength.Auto});

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