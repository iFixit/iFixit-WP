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
    public class Tool : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public const string DELIM = ",";

        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int quantity { get; set; }

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
        public string notes { get; set; }

        private string _url;
        [DataMember]
        public string url 
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    NotifyPropertyChanging("url");
                    _url = value;
                    NotifyPropertyChanged("url");
                }
            }
        }
        [DataMember]
        public string thumbnail { get; set; }

        public Tool()
            : base()
        { }

        public Tool(GHTool t)
        {
            this.FillFields(t);
        }

        public Tool(string s)
        {
            string[] delims = { DELIM };
            string[] fields = s.Split(delims, StringSplitOptions.None);
            if (fields.Length == 6)
            {
                this.type = fields[0];
                this.quantity = int.Parse(fields[1]);
                this.text = fields[2];
                this.notes = fields[3];
                this.url = fields[4];
                this.thumbnail = fields[5];
            }
        }

        public void FillFields(GHTool t)
        {
            this.type = Guide.UnescapeHtml(t.type);
            this.quantity = int.Parse(t.quantity);
            this.text = Guide.UnescapeHtml(t.text);
            this.notes = Guide.UnescapeHtml(t.notes);
            this.url = Guide.UnescapeHtml(t.url);
            this.thumbnail = Guide.UnescapeHtml(t.thumbnail);
        }

        public Uri navUri
        {
            get
            {
                return new Uri(url);
            }
        }

        public override string ToString()
        {
            return this.type + DELIM +
                   this.quantity + DELIM +
                   this.text + DELIM +
                   this.notes + DELIM +
                   this.url + DELIM +
                   this.thumbnail;
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
