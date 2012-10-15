using System.Collections.Generic;
using System.Windows.Media;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace iFixit7
{
    /*
     * A small type to hold lines inside guide steps
     */
    [Table]
    public class Lines : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Lines(GHStepLines l)
        {
            this.Text = l.text;
            this.Level = l.level;
            this.ColorString = l.bullet;
        }

        /*
         * A utility function for converting from the color specified by a string in JSON to a C# color
         */
        public static Color ConvertToColor(string col)
        {
            //setup the static dictionary
            var ColorsByJSONName = new Dictionary<string, Color>() { 
                {"black",Colors.Black}, 
                {"red",Colors.Red},
                {"orange",Colors.Orange},
                {"yellow",Colors.Yellow},
                {"green",Colors.Green},
                {"blue",Colors.Blue},
                {"violet",Colors.Purple} 
            };

            //index into it and return
            return ColorsByJSONName[col];
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        //the M hook of the 1:M of step to lines
        [Column(Name = "lineStepID")]
        private int? lineStepID { get; set; }

        //text
        private string _text;
        [Column]
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    NotifyPropertyChanging("Text");
                    _text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }

        //color
        //FIXME there is no LINQ type for color, so convert when retreived (but dont store it)
        public Color ColorBrush
        {
            get
            {
                return Lines.ConvertToColor(_colorString);
            }
            set
            {
                //this has to be something. It will be ignored though
                ColorBrush = Colors.Blue;
            }
        }
        private string _colorString;
        [Column]
        public string ColorString
        {
            get
            {
                return _colorString;
            }
            set
            {
                NotifyPropertyChanging("ColorString");
                _colorString = value;
                NotifyPropertyChanged("ColorString");
            }
        }

        //level
        //text
        private string _level;
        [Column]
        public string Level
        {
            get
            {
                return _level;
            }
            set
            {
                if (_level != value)
                {
                    NotifyPropertyChanging("Level");
                    _level = value;
                    NotifyPropertyChanged("Level");
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
