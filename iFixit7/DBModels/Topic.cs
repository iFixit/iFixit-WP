
// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace iFixit7
{
    [Table]
    [DataContract(IsReference = true)]
    public class Topic : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public override bool Equals(object obj)
        {
            Topic other = obj as Topic;

            if (obj == null || other == null)
                return false;

            return (other.Name.Equals(this.Name)) && (other.parentName.Equals(this.parentName));
        }
        public override int GetHashCode()
        {
            //this is a hack for force LINQ to use our equals method every time
            return 0;
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }


        [Column]
        public string parentName { get; set; }

        public List<Guide> Guides = new List<Guide>();

        public void AddGuide(Guide g)
        {
            g.parentName = this.Name;
            this.Guides.Add(g);
        }

        public Category Parent { get; set; }

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

        /*
         * a hook to get a shortened version of the title if the full thing is too long
         */
        public string ShortName
        {
            get
            {
                if (_name != null)
                {
                    if (_name.Length > 27)
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



        private string _description = "this is a description";
        //text
        [Column(DbType = "NText")]
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
