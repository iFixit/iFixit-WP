
// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;

namespace iFixit7
{
    [Table (Name = "AllCategories")]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Category()
        {
        }
        public Category(string name) : this()
        {
            _name = name;
        }

        //the primary key
        [Column( AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }


        
        //the 1 side of the 1:M collection of categories
        //See: http://msdn.microsoft.com/en-us/library/Bb386950(v=VS.90).aspx#Y1000
        //private EntitySet<Category> _Categories = new EntitySet<Category>();
        //[Association(Name = "CategoryCategories", Storage = "_Categories", OtherKey = "categoryId", ThisKey = "id")]
        //public ICollection<Category> Categories
        //{
        //    get { return this._Categories; }
        //    set {
        //        NotifyPropertyChanging("Categories");
        //        this._Categories.Assign(value);
        //        NotifyPropertyChanged("Categories");
        //    }

        //}

        //FIXME uncommenting this causes the mystery explosion in CreateDatabase
        //"Unable to determine SQL type for 'System.Data.Linq.EntityRef`1[iFixit7.Category]'."
        
        //the M side side of the 1:M of categories
        //FIXME do we need this? can we use id again? NO, need this here!
        [Column(Name = "IFI_Category")] private int? categoryId;
        private EntitySet<Category> _categories = new EntitySet<Category>();
        [Association(Name = "FK_Category_Category", Storage = "_categories",
            OtherKey = "categoryId", ThisKey = "Id")]
        //should these be 'ObservableCollection's?
        public ICollection<Category> Categories
        {
            get { return _categories; }
            set {
                NotifyPropertyChanging("Categories");
                _categories.Assign(value);
                NotifyPropertyChanged("Categories");
            }
        }

        //[Column]
        //private EntityRef<Category> _ParentCategory = new EntityRef<Category>();
        ////not IsForeignKey = true, ?
        //[Association(Name = "CategoryCategories", IsForeignKey = true, Storage = "_ParentCategory", ThisKey = "categoryId")]
        //public Category ParentCategory
        //{
        //    get { return this._ParentCategory.Entity; }
        //    set {
        //        NotifyPropertyChanging("ParentCategory");
        //        this._ParentCategory.Entity = value;
        //        NotifyPropertyChanged("ParentCategory");
        //    }
        //}




        //1 side of 1:M for topics
        private EntitySet<Topic> _Topics = new EntitySet<Topic>();
        [Association(Name = "CategoryToTopic", Storage = "_Topics", ThisKey = "Id", OtherKey = "topID")]
        public ICollection<Topic> Topics
        {
            get { return this._Topics; }
            set
            {
                NotifyPropertyChanging("Topics");
                this._Topics.Assign(value);
                NotifyPropertyChanged("Topics");
            }
        }



        private string _name = "";
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
