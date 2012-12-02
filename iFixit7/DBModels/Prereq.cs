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
using System.Runtime.Serialization;
using System.ComponentModel;

namespace iFixit7.DBModels
{
    [DataContract]
    public class Prereq : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public const string DELIM = ",";

        private string _text;
        [DataMember]
        public string text 
        { 
            get
            {
                return _text;
            }
            set
            {
                if (_text != value)
                {
                    NotifyPropertyChanging("text");
                    _text = value;
                    NotifyPropertyChanged("text");
                }
            }
        }

        [DataMember]
        public int guideid { get; set; }
        [DataMember]
        public string locale { get; set; }

        private string _shortName;
        public string shortName
        {
            get
            {
                if (_shortName != null && _shortName != "")
                {
                    return _shortName;
                }
                else
                {
                    return text;
                }
            }
            set
            {
                if (_shortName != value)
                {
                    NotifyPropertyChanging("shortName");
                    _shortName = value;
                    NotifyPropertyChanged("shortName");
                }
            }
        }

        public Prereq()
            : base()
        { }

        public Prereq(GHPrereq p)
        {
            this.FillFields(p);
        }
        
        public Prereq(string s)
        {
            string[] delims = { DELIM };
            string[] fields = s.Split(delims, StringSplitOptions.None);
            if (fields.Length == 3)
            {
                this.text = fields[0];
                this.guideid = int.Parse(fields[1]);
                this.locale = fields[2];
            }
        }

        public void FillFields(GHPrereq p)
        {
            this.text = Guide.UnescapeHtml(p.text);
            this.guideid = int.Parse(p.guideid);
            this.locale = Guide.UnescapeHtml(p.locale);
        }

        public Uri uri
        {
            get
            {
                return new Uri("/Guide.xaml?GuideID=" + guideid, UriKind.Relative);
            }
        }

        public override string ToString()
        {
            return this.text + DELIM +
                   this.guideid + DELIM +
                   this.locale;
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
