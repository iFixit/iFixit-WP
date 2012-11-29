using System.Collections.Generic;

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using Microsoft.Phone.Net.NetworkInformation;
using System.Diagnostics;

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
            this.Thumbnail = gh.guide.image.medium;
            this.TitleImage = gh.guide.image.standard;

            //copy over steps
            foreach (GHStep s in gh.guide.steps)
            {
                Step dbStep = new Step(s);
                dbStep.parentName = this.Title;
                this.Steps.Add(dbStep);
            }

            this.Populated = true;
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
            this.Populated = false;
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column]
        public string parentName { get; set; }

        public List<Step> Steps = new List<Step>();

        public void AddStep(Step s)
        {
            s.parentName = this.Title;
            this.Steps.Add(s);
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

                    //strip HTML formatting the hard way http://www.theukwebdesigncompany.com/articles/entity-escape-characters.php
                    var v = value.Replace("&quot;", "\"");
                    v = v.Replace("&amp;", "&");
                    v = v.Replace("&nbsp;", " ");
                    _title = v;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        /*
         * a hook to get the shortened version of the title for binding
         */
        public string ShortTitle
        {
            get
            {
                if (_title != null)
                {
                    //if _title.Length > XXX
                    //remove topic name (and leading space) from guide titles
                    return _title.Replace(" " + _topic, "");
                }
                return "";
            }
            set
            {
                ShortTitle = "";
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

        /*
         * A column to store if this topic has been populated from the web yet (AKA cached)
         */
        private bool _populated = false;
        [Column]
        public bool Populated
        {
            get
            {
                return _populated;
            }
            set
            {
                Debug.WriteLine(this.ShortTitle + ": setting populated to " + value);
                if (_populated != value)
                {
                    NotifyPropertyChanging("Populated");
                    _populated = value;
                    NotifyPropertyChanged("Populated");
                }
            }
        }

        /*
         * A hacky property to bind to to set opacity based on populated
         */
        public double PopulatedOpacity
        {
            get
            {
                if (_populated || DeviceNetworkInformation.IsNetworkAvailable)
                {
                    Debug.WriteLine(this.ShortTitle + ": populated");
                    return 1.0;
                }
                Debug.WriteLine(this.ShortTitle + ": UNpopulated");
                return 0.5;
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
