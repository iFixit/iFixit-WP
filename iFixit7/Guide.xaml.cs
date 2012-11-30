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

namespace iFixit7
{
    public partial class GuideView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
    {
        /*
         * The view model for a guide view
         */
        public class GuideViewModel
        {
            //holds what goes in each column
            public class ColContent
            {
                public string Title { get; set; }
                public string Image1 { get; set; }
                public string Image2 { get; set; }
                public string Image3 { get; set; }
                public ObservableCollection<Lines> Lines { get; set; }

                public ColContent(Step s)
                {
                    this.Title = "Step " + s.StepIndex;
                    //FIXME this is probably bad. We are always assuming there is a .standard availible
                    this.Image1 = s.Image1 + ".standard";
                    this.Image2 = s.Image2 + ".standard";
                    this.Image3 = s.Image3 + ".standard";

                    Lines = new ObservableCollection<Lines>();
                    foreach (Lines l in s.Lines)
                    {
                        Lines.Add(l);
                    }
                }
            }

            public Guide SourceGuide;

            private ColContent infoTab = null;
            public ObservableCollection<ColContent> ColHeaders { get; set; }
            public string GuideTitle { get; set; }
            public string GuideTopic { get; set; }

            /*
             * Populate this VM with a pre-existing Guide
             */
            public GuideViewModel(Guide g)
            {
                ColHeaders = new ObservableCollection<ColContent>();

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

                //FIXME manually add info page
                AddInfoTab(g);

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
            private void AddInfoTab(Guide g)
            {
                if (infoTab != null)
                {
                    ColHeaders.Remove(infoTab);
                }

                infoTab = new ColContent(new Step());
                infoTab.Title = "Info";
                infoTab.Image1 = g.TitleImage;

                Lines l = new Lines();
                l.Text = g.Summary;
                l.ColorString = "white";
                infoTab.Lines.Add(l);

                ColHeaders.Add(infoTab);
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
         * Look at the tag from the sending Image to figure out which was touched.
         */
        private void Img_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Image src = sender as Image;
            Debug.WriteLine("tapped image " + src.Tag);

            //get the proper source url
            String srcUrl = (src.Source as BitmapImage).UriSource.ToString();

            //modify it to get the huge version
            //srcUrl = srcUrl.Replace(".standard", ".huge");
            srcUrl = srcUrl.Replace(".standard", "");
            
            //FIXME navigate to fullscreen image w/ URL (just URL?)
            NavigationService.Navigate(new Uri("/FullscreenImage.xaml?ImgURI=" + srcUrl,
                UriKind.Relative));
        }

        //occurs when a line is tapped
        private void GuideLine_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            WP7_Mango_HtmlTextBlockControl.HtmlTextBlock tb = sender as WP7_Mango_HtmlTextBlockControl.HtmlTextBlock;
            int firstOpen = tb.VisibleText.IndexOf("<");
            int firstClose = tb.VisibleText.IndexOf(">");
            string linkEnd;
            string link;
            if (firstOpen >= 0 && firstClose >= 0)
            {
                int length = firstClose - firstOpen;
                linkEnd = tb.VisibleText.Substring(firstOpen + 1, length - 1);
                link = "http://www.ifixit.com" + linkEnd;
                WebBrowserTask wbt = new WebBrowserTask();
                wbt.Uri = new Uri(link);
                wbt.Show();
            }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");
            this.guideID = this.NavigationContext.QueryString["GuideID"];
            Debug.WriteLine("\tgot guide id = " + guideID);

            //if the VM is already populated, don't reload it
            if (vm != null)
                return;
            
            //if online, do an api call. The callback will be fired to populate the view
            if (DeviceNetworkInformation.IsNetworkAvailable)
            {
                Debug.WriteLine("onine. Querying for new guide content");

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
            this.DataContext = vm;

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
