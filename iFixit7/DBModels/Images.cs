/*
 * The DB table to hold info about all images.
 */

using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace iFixit7
{
    [Table]
    public class Images : INotifyPropertyChanged, INotifyPropertyChanging
    {
        //the primary key
        [ColumnAttribute(Storage = "id", AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int id
        {
            get { return id; }
            set { id = value; }
        }

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