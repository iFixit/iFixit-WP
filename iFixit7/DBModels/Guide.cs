using System.Collections.Generic;

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace iFixit7
{
    [Table]
    public class Guide : INotifyPropertyChanged, INotifyPropertyChanging
    {
        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        //FIXME add collection of steps!

        //the M hook of the 1:M of topics to guides
        [Column(Name = "guideGroupID")]
        private int? guideGroupID { get; set; }


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

        private string _subject;
        [Column]
        public string Subject
        {
            get
            {
                return _subject;
            }
            set
            {
                if (_subject != value)
                {
                    NotifyPropertyChanging("Subject");
                    _subject = value;
                    NotifyPropertyChanged("Subject");
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
