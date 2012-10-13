
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
        public int Id
        {
            get; set;
        }
        
        //the M side side of the 1:M of categories
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
