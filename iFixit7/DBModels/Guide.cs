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
        [ColumnAttribute(Storage = "id", AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int id
        {
            get { return id; }
            set { id = value; }
        }

        private string _information;
        [Column]
        public string Information
        {
            get
            {
                return _information;
            }
            set
            {
                if (_information != value)
                {
                    NotifyPropertyChanging("Information");
                    _information = value;
                    NotifyPropertyChanged("Information");
                }
            }
        }

        /*
        private List<Step> _steps;
        [Column]
        public List<Step> Steps
        {
            get
            {
                return _steps;
            }
            set
            {
                if (_steps != value)
                {
                    NotifyPropertyChanging("Steps");
                    _steps = value;
                    NotifyPropertyChanged("Steps");
                }
            }
        }
         * */

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
