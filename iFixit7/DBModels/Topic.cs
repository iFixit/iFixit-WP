
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
        public Topic()
        {
            //_Guides = new EntitySet<Guide>();
            _TopID = new EntityRef<Category>();
        }

        //the primary key
        [ColumnAttribute(Storage = "id", AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int id
        {
            get { return id; }
            set { id = value; }
        }

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

        private EntityRef<Category> _TopID;
        [Association(Storage = "_TopID", ThisKey = "TopID")]
        public Category TopID
        {
            get { return this._TopID.Entity; }
            set { this._TopID.Entity = value; }
        }

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

        //a 1-1 ref for the image associated with this topic
        private EntityRef<Images> _Image;
        [Association(Storage = "_Image", ThisKey = "id")]
        public Images Image
        {
            get { return this._Image.Entity; }
            set
            {
                NotifyPropertyChanging("Image");
                this._Image.Entity = value;
                NotifyPropertyChanged("Image");
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
