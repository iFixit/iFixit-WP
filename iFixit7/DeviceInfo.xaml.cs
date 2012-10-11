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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace iFixit7
{
    public partial class DeviceInfo : PhoneApplicationPage
    {
        public TopicInfoViewModel vm;

        private string navTopicName;

        public DeviceInfo()
        {
            InitializeComponent();

            vm = new TopicInfoViewModel(navTopicName);

            this.DataContext = vm;
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
            ji.populateDeviceInfo(InfoPano.Title.ToString(), populateUI);

            //clear the loading bar when we are done loading data
            //this.LoadingBar.IsVisible = false;
        }
        private bool populateUI(DeviceInfoHolder devInfo){
            Debug.WriteLine("filling in device info ui...");

            return true;

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

    public class TopicInfoViewModel
    {
        public String Name { get; set; }

        public TopicInfoViewModel(string name)
        {
            this.Name = name;
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