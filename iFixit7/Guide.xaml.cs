using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
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
using System.Windows.Data;
using Microsoft.Phone;
using System.IO;

namespace iFixit7
{
    public partial class GuideView : PhoneApplicationPage, INotifyPropertyChanged, INotifyPropertyChanging
    {
        //add handlers so when we change this, it updates in the main view
        private Guide _sourceGuide;
        public Guide SourceGuide
        {
            get
            {
                return _sourceGuide;
            }
            set
            {
                if (_sourceGuide != value)
                {
                    NotifyPropertyChanging("SourceGuide");
                    _sourceGuide = value;
                    NotifyPropertyChanged("SourceGuide");
                }
            }
        }

        string guideID = "";

        public GuideView()
        {
            InitializeComponent();

            this.DataContext = SourceGuide;
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
            this.DataContext = SourceGuide;

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
