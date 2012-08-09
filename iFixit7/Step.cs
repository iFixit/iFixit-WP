using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace iFixit7
{
    [Table]
    public class Step : INotifyPropertyChanged, INotifyPropertyChanging
    {
        [Column]
        private string _metaInfo;

        public string MetaInfo
        {
            get
            {
                return _metaInfo;
            }
            set
            {
                if (_metaInfo != value)
                {
                    NotifyPropertyChanging("MetaInfo");
                    _metaInfo = value;
                    NotifyPropertyChanged("MetaInfo");
                }
            }
        }

        private List<Image> _images;

        [Column]
        public List<Image> Images
        {
            get
            {
                return _images;
            }
            set
            {
                if (_images != value)
                {
                    NotifyPropertyChanging("Images");
                    _images = value;
                    NotifyPropertyChanged("Images");
                }
            }
        }

        private List<string> _descriptions;

        [Column]
        public List<string> Descriptions
        {
            get
            {
                return _descriptions;
            }
            set
            {
                if (_descriptions != value)
                {
                    NotifyPropertyChanging("Descriptions");
                    _descriptions = value;
                    NotifyPropertyChanged("Descriptions");
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
