﻿using System.Collections.Generic;

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace iFixit7
{
    [Table]
    public class Guide : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Guide() : base()
        {}

        /*
         * This constructor is for making a new Guide from a GuideHolder
         */
        public Guide(GuideHolder gh)
        {
            this.FillFields(gh);
        }

        public void FillFields(GuideHolder gh){
            //fill in fields
            this.Title = gh.guide.title;
            this.Topic = gh.topic;
            this.Summary = gh.guide.summary;
            this.URL = gh.url;
            this.GuideID = gh.guideid;
            this.Thumbnail = gh.guide.image.thumbnail;
            this.TitleImage = gh.guide.image.standard;

            //copy over steps
            foreach (GHStep s in gh.guide.steps)
            {
                this.Steps.Add(new Step(s));
            }
        }

        /*
         * Fills in the fields of this object from the incomplete DeviceInfo model of a guide
         */
        public void FillFieldsFromDeviceInfo(string top, DIGuides g)
        {
            this.Title = g.title;
            this.Topic = top;
            this.Summary = "";
            this.URL = g.url;
            this.GuideID = g.guideid;
            this.Thumbnail = g.thumbnail;
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        //the M hook of the 1:M of topics to guides
        [Column(Name = "guideGroupID")]
        private int? guideGroupID;


        //1 side of 1:M for the collection of guides
        private EntitySet<Step> _steps = new EntitySet<Step>();
        [Association(Name = "GuideToSteps", Storage = "_steps", ThisKey = "Id", OtherKey = "stepGroupID")]
        public ICollection<Step> Steps
        {
            get { return this._steps; }
            set
            {
                NotifyPropertyChanging("Steps");
                this._steps.Assign(value);
                NotifyPropertyChanged("Steps");
            }
        }

        private string _title;
        [Column]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    NotifyPropertyChanging("Title");
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        //this should probably be an entity ref or something, not just a string
        private string _topic;
        [Column]
        public string Topic
        {
            get
            {
                return _topic;
            }
            set
            {
                if (_topic != value)
                {
                    NotifyPropertyChanging("Topic");
                    _topic = value;
                    NotifyPropertyChanged("Topic");
                }
            }
        }

        private string _summary;
        [Column]
        public string Summary
        {
            get
            {
                return _summary;
            }
            set
            {
                if (_summary != value)
                {
                    NotifyPropertyChanging("Summary");
                    _summary = value;
                    NotifyPropertyChanged("Summary");
                }
            }
        }

        private string _url;
        [Column]
        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    NotifyPropertyChanging("URL");
                    _url = value;
                    NotifyPropertyChanged("URL");
                }
            }
        }

        private string _guideID;
        [Column]
        public string GuideID
        {
            get
            {
                return _guideID;
            }
            set
            {
                if (_guideID != value)
                {
                    NotifyPropertyChanging("GuideID");
                    _guideID = value;
                    NotifyPropertyChanged("GuideID");
                }
            }
        }

        //the mini/thumbnail photo [thumbnail]
        private string _thumbnail;
        [Column]
        public string Thumbnail
        {
            get
            {
                return _thumbnail;
            }
            set
            {
                if (_thumbnail != value)
                {
                    NotifyPropertyChanging("Thumbnail");
                    _thumbnail = value;
                    NotifyPropertyChanged("Thumbnail");
                }
            }
        }

        //a larger title photo [standard] (small?)
        private string _titleImage;
        [Column]
        public string TitleImage
        {
            get
            {
                return _titleImage;
            }
            set
            {
                if (_titleImage != value)
                {
                    NotifyPropertyChanging("TitleImage");
                    _titleImage = value;
                    NotifyPropertyChanged("TitleImage");
                }
            }
        }
        

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

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
