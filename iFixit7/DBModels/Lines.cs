using System.Collections.Generic;
using System.Windows.Media;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Diagnostics;

namespace iFixit7
{
    /*
     * A small type to hold lines inside guide steps
     */
    [Table]
    public class Lines : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Lines()
        {
        }
        public Lines(GHStepLines l)
        {
            this.Text = l.text;
            this.Level = l.level.ToString();
            this.ColorString = l.bullet;
        }

        /*
         * A utility function for converting from the color specified by a string in JSON to a C# color
         */
        public static Color ConvertToColor(string col)
        {
            //setup the static dictionary
            var ColorsByJSONName = new Dictionary<string, Color>() { 
                //{"black",Colors.Black}, 
                {"black",Colors.DarkGray}, 
                
                {"red",Colors.Red},
                {"orange",Colors.Orange},
                {"yellow",Colors.Yellow},
                {"green",Colors.Green},
                {"blue",Colors.Blue},
                {"violet",Colors.Purple},

                {"white", Colors.White},

                //what should these be?
                {"icon_note", Colors.Magenta},
                {"icon_reminder", Colors.Cyan},
                {"icon_caution", Colors.Brown}
            };

            //index into it and return
            return ColorsByJSONName[col];
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        /*
        //the M hook of the 1:M of step to lines
        [Column(Name = "lineStepID")]
        private int? lineStepID { get; set; }
        */

        [Column]
        public string parentName { get; set; }


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
        public SolidColorBrush ColorBrush
        {
            get
            {
                return new SolidColorBrush(Lines.ConvertToColor(_colorString));
            }
            set
            {
                //this has to be something. It will be ignored though
                ColorBrush = null;
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

        // a modified level for data binding
        public string DrawingLevelWidth
        {
            get
            {
                if(_level != null)
                    return (int.Parse(_level) * 14).ToString();
                return "0";
            }
            set
            {
                DrawingLevelWidth = "";
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
