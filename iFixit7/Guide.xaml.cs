using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Phone.Controls;
using System.Diagnostics;

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
                public string Title;
                public string Image1, Image2, Image3;
                public ObservableCollection<Lines> Lines { get; set; }

                public ColContent(Step s)
                {
                    this.Title = s.StepIndex;
                    this.Image1 = s.Image1;
                    this.Image2 = s.Image2;
                    this.Image3 = s.Image3;

                    Lines = new ObservableCollection<Lines>();
                    foreach (Lines l in s.Lines)
                    {
                        Lines.Add(l);
                    }
                }
                override public string ToString()
                {
                    return "Step " + Title;
                }
            }

            private Guide SourceGuide;

            public ObservableCollection<ColContent> ColHeaders { get; set; }
            public string Title = "asdasd";

            /*
             * Populate this VM with a pre-existing Guide
             */
            public GuideViewModel(Guide g)
            {
                SourceGuide = g;

                ColHeaders = new ObservableCollection<ColContent>();

                UpdateContentFromGuide(g);
            }

            /*
             * Update the contents of this VM from the DB
             */
            public void UpdateContentFromGuide(Guide g)
            {
                foreach(Step s in g.Steps)
                {
                    ColHeaders.Add(new ColContent(s));
                }
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
            //LoadingBarInfo.Visibility = System.Windows.Visibility.Collapsed;
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
