
// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Runtime.Serialization;

namespace iFixit7
{
    [Table(Name = "AllCategories")]
    [DataContract(IsReference = true)]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public override bool Equals(object obj)
        {
            Category other = obj as Category;

            if (obj == null || other == null)
                return false;

            //Debug.WriteLine("myname = " + this.Name + " other name = " + other.Name);
            return (other.Name.Equals(this.Name)) && (other.parentName.Equals(this.parentName));
        }

        public override int GetHashCode()
        {
            //this is a hack for force LINQ to use our equals method every time
            return 0;
        }



        public Category()
        {
        }
        public Category(string name) : this()
        {
            _name = name;
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        [DataMember]
        public int Id
        {
            get; set;
        }

        public Uri url
        {
            get
            {
                return new Uri("http://www.ifixit.com/Device/" + _name.Replace(" ", "_"));
            }
        }

        [Column]
        [DataMember]
        public string parentName { get; set; }

        [DataMember]
        public Category Parent { get; set; }

        [DataMember]
        public List<Category> Categories = new List<Category>();
        [DataMember]
        public List<Topic> Topics = new List<Topic>();

        public void AddCategory(Category c)
        {
            c.parentName = this.Name;
            this.Categories.Add(c);
        }

        public void AddTopic(Topic t)
        {
            t.parentName = this.Name;
            this.Topics.Add(t);
        }


        private string _name = "";
        [Column]
        [DataMember]
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

        /*
         * a hook to get a shortened version of the title if the full thing is too long
         */
        public string ShortName
        {
            get
            {
                if (_name != null)
                {
                    if(_name.Length > 27)
                        return _name.Replace(parentName + " ", "");
                    return _name;
                }
                return "";
            }
            set
            {
                ShortName = "";
            }
        }

        private string _thumbnail = "";
        [Column]
        [DataMember]
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
