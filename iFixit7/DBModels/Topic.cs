
// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;

namespace iFixit7
{
    [Table]
    public class Topic : INotifyPropertyChanged, INotifyPropertyChanging
    {
        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        //sub-guides
        /*
        private EntitySet<Guide> _Guides;
        [Association(Storage = "_Guides", OtherKey = "id")]
        public EntitySet<Guide> Guides
        {
            get { return this._Guides; }
            set
            {
                NotifyPropertyChanging("Guides");
                this._Guides.Assign(value);
                NotifyPropertyChanged("Guides");
            }
        }
         */

        ////The M side of the 1:M of categories to guides
        [Column(Name = "topID")]
        private int? topID { get; set; }


        private string _name;
        [Column]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    NotifyPropertyChanging("Name");
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _description;
        [Column]
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (_description != value)
                {
                    NotifyPropertyChanging("Description");
                    _description = value;
                    NotifyPropertyChanged("Description");
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
                if (_populated != value)
                {
                    NotifyPropertyChanging("Populated");
                    _populated = value;
                    NotifyPropertyChanged("Populated");
                }
            }
        }

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        /*
        private string _imageUrl;
        [Column]
        public string ImageURL
        {
            get
            {
                return _imageUrl;
            }
            set
            {
                if (_imageUrl != value)
                {
                    NotifyPropertyChanging("ImageURL");
                    _imageUrl = value;
                    NotifyPropertyChanged("ImageURL");
                }
            }
        }
         */

        /*
        private List<Guide> _guides;

        [Column]
        public List<Guide> Guides
        {
            get
            {
                return _guides;
            }
            set
            {
                if (_guides != value)
                {
                    NotifyPropertyChanging("Guides");
                    _guides = value;
                    NotifyPropertyChanged("Guides");
                }
            }
        }
        */

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
