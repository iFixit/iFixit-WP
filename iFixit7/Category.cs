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
    public class CategoryDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public CategoryDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a table for the categories.
        public Table<Category> CategoriesTable;
    }

    [Table]
    public class Category : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Category()
        {
            _categories = new List<Category>();
            _devices = new List<Device>();
        }

        public Category(string name) : this()
        {
            _name = name;
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

        private List<Device> _devices;

        [Column]
        public List<Device> Devices
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
