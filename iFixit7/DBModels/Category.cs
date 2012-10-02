
// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;

namespace iFixit7
{
    [Table]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Category()
        {
            //no initialization needed for 1->many's, right?
            /*
            _categories = new List<Category>();
            _devices = new List<Topic>();
             */
            _Categories = new EntitySet<Category>();
            _ColID = new EntityRef<Category>();
            _Topics = new EntitySet<Topic>();
        }

        public Category(string name) : this()
        {
            _name = name;
        }

        //the primary key
        [ColumnAttribute(Storage = "id", AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int id
        {
            get { return id; }
            set { id = value; }
        }

        //sub-categories collection
        //See: http://msdn.microsoft.com/en-us/library/Bb386950(v=VS.90).aspx#Y1000
        private EntitySet<Category> _Categories;
        [Association(Storage = "_Categories", OtherKey = "ColID")]
        public EntitySet<Category> Categories
        {
            get { return this._Categories; }
            set {
                NotifyPropertyChanging("Categories");
                this._Categories.Assign(value);
                NotifyPropertyChanged("Categories");
            }
        }

        //counterpart of the above collection
        [Column]
        private EntityRef<Category> _ColID;
        [Association(Storage = "_ColID", ThisKey = "ColID")]
        public Category ColID
        {
            get { return this._ColID.Entity; }
            set { this._ColID.Entity = value; }
        }

        //sub-topics
        private EntitySet<Topic> _Topics;
        [Association(Storage = "_Topics", OtherKey = "TopID")]
        public EntitySet<Topic> Topics
        {
            get { return this._Topics; }
            set
            {
                NotifyPropertyChanging("Topics");
                this._Topics.Assign(value);
                NotifyPropertyChanged("Topics");
            }
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

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        /*
        private List<Category> _categories;

        [Column]
        public List<Category> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                if (_categories != value)
                {
                    NotifyPropertyChanging("Categories");
                    _categories = value;
                    NotifyPropertyChanged("Categories");
                }
            }
        }

        private List<Topic> _devices;

        [Column]
        public List<Topic> Devices
        {
            get
            {
                return _devices;
            }
            set
            {
                if (_devices != value)
                {
                    NotifyPropertyChanging("Devices");
                    _devices = value;
                    NotifyPropertyChanged("Devices");
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
