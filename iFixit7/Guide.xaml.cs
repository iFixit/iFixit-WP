using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using iFixit7.DBModels;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace iFixit7
{
    public class GuideTemplateSelector : DataTemplateSelector
    {
        public DataTemplate InfoTab
        {
            get;
            set;
        }

        public DataTemplate StepTab
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            TabContainer tab = item as TabContainer;
            if (tab != null)
            {
                if (tab.Type == "Info")
                {
                    return InfoTab;
                }
                else if (tab.Type == "Step")
                {
                    return StepTab;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }

    public class TabContainer
    {
        public string Type { get; set; }
    }

    public partial class GuideView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
    {
        /*
         * The view model for a guide view
         */
        public class GuideViewModel
        {

            public class InfoTab : TabContainer
            {
                public string Title { get; set; }
                public string TitleImage { get; set; }
                public string Introduction { get; set; }
                public string Difficulty { get; set; }
                public string diffText
                {
                    get
                    {
                        return "Difficulty: " + Difficulty;
                    }
                }
                public string Author { get; set; }
                public string authorText
                {
                    get
                    {
                        return "Author: " + Author;
                    }
                }
                public List<Part> Parts { get; set; }
                public List<Prereq> Prereqs { get; set; }
                public List<Tool> Tools { get; set; }
                public Visibility hasParts
                {
                    get
                    {
                        if (Parts != null && Parts.Count > 0)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    }
                }
                public Visibility hasPrereqs
                {
                    get
                    {
                        if (Prereqs != null && Prereqs.Count > 0)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    }
                }
                public Visibility hasTools
                {
                    get
                    {
                        if (Tools != null && Tools.Count > 0)
                        {
                            return Visibility.Visible;
                        }
                        else
                        {
                            return Visibility.Collapsed;
                        }
                    }
                }

                public void createShortNames(string topic)
                {
                    if (Prereqs != null)
                    {
                        foreach (Prereq p in Prereqs)
                        {
                            p.shortName = p.text.Replace(topic + " ", "");
                        }
                    }
                }
            }


            //holds what goes in each column
            public class ColContent : TabContainer
            {
                public string Title { get; set; }
                public string EmbedURL { get; set; }
                public System.Windows.Visibility EmbedVisibility { get; set; }
                public string Image1 { get; set; }
                public string Image2 { get; set; }
                public string Image3 { get; set; }
                public ObservableCollection<Lines> Lines { get; set; }

                public ColContent(Step s)
                {
                    this.Title = "Step " + s.StepIndex;

                    //check type
                    if (s.MediaType == Step.MediaType_Embed)
                    {

                        EmbedVisibility = System.Windows.Visibility.Visible;
                        EmbedURL = s.EmbedURL;
                    }
                    //else if (s.MediaType == Step.MediaType_Image)
                    else
                    {
                        EmbedVisibility = System.Windows.Visibility.Collapsed;
                        //FIXME this is probably bad. We are always assuming there is a .standard availible
                        this.Image1 = s.Image1 + ".standard";
                        this.Image2 = s.Image2 + ".standard";
                        this.Image3 = s.Image3 + ".standard";
                    }

                    Lines = new ObservableCollection<Lines>();
                    foreach (Lines l in s.Lines)
                    {
                        Lines.Add(l);
                    }
                    this.Type = "Step";
                }
            }

            public Guide SourceGuide;

            public ObservableCollection<TabContainer> ColHeaders { get; set; }
            public string GuideTitle { get; set; }
            public string GuideTopic { get; set; }
            public string GuideID { get; set; }

            /*
             * Populate this VM with a pre-existing Guide
             */
            public GuideViewModel(Guide g)
            {
                ColHeaders = new ObservableCollection<TabContainer>();

                if (g == null)
                    return;

                UpdateContentFromGuide(g);
            }

            /*
             * Update the contents of this VM from the DB
             */
            public void UpdateContentFromGuide(Guide g)
            {
                SourceGuide = g;

                this.GuideTitle = g.ShortTitle;
                this.GuideTopic = g.Topic;
                this.GuideID = g.GuideID;

                //FIXME manually add info page
                ColHeaders.Add(BuildInfoTab(g));

                foreach(Step s in g.Steps)
                {
                    ColHeaders.Add(new ColContent(s));
                }
            }

            /*
             * Generates a fake entry for the list of steps to hold information about the guide.
             * Not an ideal solution... Should probably build an entire PivotItem and insert it at the
             * start of GuidePivot.
             */
            private InfoTab BuildInfoTab(Guide g)
            {
                InfoTab infoTab = new InfoTab();
                infoTab.Type = "Info";
                infoTab.Title = "Info";
                infoTab.TitleImage = g.TitleImage;

                if (g.Introduction != null && g.Introduction.Length > 0)
                {
                    infoTab.Introduction = g.Introduction.Replace("<p>", "").Replace("</p>", "");
                }

                if (g.Difficulty != null && g.Difficulty.Length > 0)
                {
                    infoTab.Difficulty = g.Difficulty;
                }

                if (g.Author != null && g.Author.Length > 0)
                {
                    infoTab.Author = g.Author;
                }
                
                if (g.Parts != null && g.Parts.Length > 0)
                {
                    infoTab.Parts = g.parts;
                }

                if (g.Prereqs != null && g.Prereqs.Length > 0)
                {
                    infoTab.Prereqs = g.prereqs;
                }

                if (g.Tools != null && g.Tools.Length > 0)
                {
                    infoTab.Tools = g.tools;
                }
                infoTab.createShortNames(g.parentName);
                return infoTab;
            }
        }

        //add handlers so when we change this, it updates in the main view
        private GuideViewModel _vm;
        public GuideViewModel vm
        {
            get
            {
                return _vm;
            }
            set
            {
                if (_vm != value)
                {
                    NotifyPropertyChanging("vm");
                    _vm = value;
                    NotifyPropertyChanged("vm");
                }
            }
        }


        string guideID = "";
        private Guide SourceGuide;

        public GuideView()
        {
            InitializeComponent();

            vm = null;

            this.DataContext = vm;
        }

        /*
         * Fires when someone taps on the embedded frame
         */
        private void Embed_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                //open the browser
                string target = (sender as Grid).Tag.ToString();
                WebBrowserTask wbt = new WebBrowserTask() { Uri = new Uri(target, UriKind.Absolute) };
                wbt.Show();
            }
        }

        /*
         * Look at the tag from the sending Image to figure out which was touched.
         */
        private void Img_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image src = sender as Image;
            Debug.WriteLine("tapped image " + src.Tag);

            //get the proper source url
            String srcUrl = (src.Source as BitmapImage).UriSource.ToString();

            //modify it to get the huge version
            srcUrl = srcUrl.Replace(".standard", "");
            
            //FIXME navigate to fullscreen image w/ URL (just URL?)
            NavigationService.Navigate(new Uri("/FullscreenImage.xaml?ImgURI=" + srcUrl,
                UriKind.Relative));
        }

        //occurs when a line is tapped
        TextBlock tapped = null;
        private void GuideLine_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            base.OnTap(e);

            //WP7_Mango_HtmlTextBlockControl.HtmlTextBlock tb = sender as WP7_Mango_HtmlTextBlockControl.HtmlTextBlock;
            var selectedOver = sender as ListBox;

            if (selectedOver == null)
                return;

            Lines selected = selectedOver.SelectedItem as Lines;

            int firstOpen = selected.Text.IndexOf("<a href=\"") + "<a href=\"".Length;
            if (firstOpen > selected.Text.Length)
                return;
            int firstClose = selected.Text.IndexOf("\"", firstOpen);
            string linkEnd;
            string link;

            if (firstOpen >= 0 && firstClose >= 0)
            {
                int length = firstClose - firstOpen;
                linkEnd = selected.Text.Substring(firstOpen, length);
                if (linkEnd.Contains("www.") || linkEnd.Contains("http://"))
                {
                    link = linkEnd;
                }
                else
                {
                    link = "http://www.ifixit.com" + linkEnd;
                }

                var t = (e.OriginalSource as TextBlock);
                if (t != null)
                {
                    t.Foreground = App.Current.Resources["PhoneAccentBrush"] as Brush;
                    tapped = t;
                }

                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri(link);
                wbt.Show();
            }

            //selectedOver.Foreground = App.Current.Resources["PhoneForegroundBrush"] as Brush;
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.State[vm.GuideID] = GuidePivot.SelectedIndex;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");
            this.guideID = this.NavigationContext.QueryString["GuideID"];
            Debug.WriteLine("\tgot guide id = " + guideID);

            if (tapped != null)
                tapped.Foreground = App.Current.Resources["PhoneForegroundBrush"] as Brush;

            //if the VM is already populated, don't reload it
            if (vm != null)
                return;


            int numCols = -1;
            string key = guideID;
            //if online, do an api call. The callback will be fired to populate the view
            if (DeviceNetworkInformation.IsNetworkAvailable && !this.State.ContainsKey(key))
            {
                Debug.WriteLine("online. Querying for new guide content");

                vm = new GuideViewModel(SourceGuide);
                this.DataContext = vm;

                new JSONInterface2().populateGuideView(this.guideID, insertGuideIntoDB);
            }
            //if not online, populate view from DB
            else
            {
                Debug.WriteLine("offline. Using DB for guide content");
                using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
                {
                    //get the guide, if it already exists
                    //SourceGuide = db.GuidesTable.SingleOrDefault(g => g.GuideID == this.guideID);
                    SourceGuide = DBHelpers.GetCompleteGuide(this.guideID, db);
                    vm = new GuideViewModel(SourceGuide);

                    //force view model to update
                    //vm.UpdateContentFromGuide(SourceGuide);
                    this.DataContext = vm;

                    //hide the loading bar
                    LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            
            if (SourceGuide != null)
            {
                numCols = SourceGuide.Steps.Count + 1;
            }
            // if there is a saved index, re-navigate to it
            if (key != null && this.State.ContainsKey(key))
            {
                int selectedTabIndex = (int)this.State[key];
                if (0 <= selectedTabIndex && selectedTabIndex < numCols)
                {
                    GuidePivot.SelectedIndex = selectedTabIndex;
                }
            }
        }

        public bool insertGuideIntoDB(GuideHolder guide){
            //convert the GuideHolder we got into a DB object

            Debug.WriteLine("inserting guide " + guide.guide.title + " into db from net");

            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                //SourceGuide = db.GuidesTable.SingleOrDefault(g => g.GuideID == this.guideID);
                SourceGuide = DBHelpers.GetCompleteGuide(this.guideID, db);

                if (SourceGuide == null)
                {
                    Debug.WriteLine("\tdid not find old");
                    SourceGuide = new Guide(guide);
                    db.GuidesTable.InsertOnSubmit(SourceGuide);
                }
                else
                {
                    Debug.WriteLine("\tfound old");
                    SourceGuide.FillFields(guide);
                }

                InsertStepsAndLines(SourceGuide, guide.guide.steps, db);

                //insert it into the DB
                SourceGuide.Populated = true;

                db.SubmitChanges();
            }

            //force view model to update
            vm.UpdateContentFromGuide(SourceGuide);
            this.gTitle.Text = vm.GuideTitle;
            this.gTopic.Text = vm.GuideTopic;
            //PivotItem infoPivot = buildInfoPivotItem();
            //GuidePivot.Items.Add(infoPivot);

            this.DataContext = vm;
            //GuidePivot.ItemsSource = temp;
            //hide the loading bar
            LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
            return true;
        }

        /*
         * Iterate through all steps and their lines, inserting them into the DB
         */
        private void InsertStepsAndLines(Guide parentGuide, GHStep[] steps, iFixitDataContext db)
        {
            //loop through all steps and attempt to insert them
            foreach (GHStep newStep in steps)
            {
                //see if it exists
                //Step sOld = db.StepsTable.FirstOrDefault(s => s.Title == newStep.title);
                Step sOld = DBHelpers.GetCompleteStep(parentGuide.GuideID, newStep.number, db);
                if (sOld == null)
                {
                    //insert it
                    sOld = new Step(newStep);
                    sOld.parentName = parentGuide.GuideID;
                    db.StepsTable.InsertOnSubmit(sOld);
                }
                else
                {
                    //update it
                    sOld.FillFields(newStep);
                }

                //go through all lines
                foreach (GHStepLines newLine in newStep.lines)
                {
                    Lines oldLine = db.LinesTable.FirstOrDefault(l => l.parentName == sOld.parentName + sOld.StepIndex);
                    if (oldLine == null)
                    {
                        //insert it
                        Lines l = new Lines(newLine);
                        l.parentName = sOld.parentName + sOld.StepIndex;
                        db.LinesTable.InsertOnSubmit(l);
                    }
                    else
                    {
                        //add it
                        oldLine.FillFields(newLine);
                        oldLine.parentName = sOld.parentName + sOld.StepIndex;
                    }
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion

        
    }
}
