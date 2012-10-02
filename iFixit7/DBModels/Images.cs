/*
 * The DB table to hold info about all images.
 */

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

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace iFixit7
{
    [Table]
    public class Image : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private string _baseUrl;
        [Column]
        public string baseUrl
        {
            get
            {
                return _baseUrl;
            }
            set
            {
                if (_baseUrl != value)
                {
                    NotifyPropertyChanging("BaseURL");
                    _baseUrl = value;
                    NotifyPropertyChanged("BaseURL");
                }
            }
        }

        private int _imageID;
        [Column]
        public int ImageID
        {
            get
            {
                return _imageID;
            }
            set
            {
                if (_imageID != value)
                {
                    NotifyPropertyChanging("ImageID");
                    _imageID = value;
                    NotifyPropertyChanged("ImageID");
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