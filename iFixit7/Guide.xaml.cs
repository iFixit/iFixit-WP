using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System;

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
                SourceGuide = g;

                GuideTitle = g.Title;
                GuideTopic = g.Topic;

                ColHeaders = new ObservableCollection<ColContent>();

                //FIXME manually add info page
                AddInfoTab(g);

                UpdateContentFromGuide(g);
            }

            /*
             * Update the contents of this VM from the DB
             */
            public void UpdateContentFromGuide(Guide g)
            {
                AddInfoTab(g);
                foreach(Step s in g.Steps)
                {
                    ColHeaders.Add(new ColContent(s));
                }
            }

            /*
             * Generates a fake entry for the list of steps to hold information about the guide.
             * Not an ideal solution... Should probabky build an entire PivotItem and insert it at the
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

            this.DataContext = vm;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");
            this.guideID = this.NavigationContext.QueryString["GuideID"];
            Debug.WriteLine("\tgot guide id = " + guideID);

            //get the guide, if it already exists
            SourceGuide = App.mDB.GuidesTable.SingleOrDefault(g => g.GuideID == this.guideID);
            if (SourceGuide != null)
            {
                Debug.WriteLine("\tgot guide title = " + SourceGuide.Title);

                //clear the existing Guide from the DB
                App.mDB.GuidesTable.DeleteOnSubmit(SourceGuide);
                App.mDB.SubmitChanges();
            }
            vm = new GuideViewModel(SourceGuide);

            //api call. The callback will be fired to populate the view
            new JSONInterface2().populateGuideView(this.guideID, insertGuideIntoDB);
        }
        public bool insertGuideIntoDB(GuideHolder guide){
            //convert the GuideHolder we got into a DB object
            SourceGuide = new Guide(guide);

            //insert it into the DB
            App.mDB.GuidesTable.InsertOnSubmit(SourceGuide);
            App.mDB.SubmitChanges();

            //force view model to update
            vm.UpdateContentFromGuide(SourceGuide);
            this.DataContext = vm;

            //hide the loading bar
            LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
            return true;
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
