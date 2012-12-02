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
    public class Part : INotifyPropertyChanged, INotifyPropertyChanging
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
        public string notes { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int quantity { get; set; }
        [DataMember]
        public string url { get; set; }

        public Part()
            : base()
        { }

        public Part(GHPart p)
        {
            this.FillFields(p);
        }

        public Part(string s)
        {
            string[] delims = { DELIM };
            string[] fields = s.Split(delims, StringSplitOptions.None);
            if (fields.Length == 5)
            {
                this.text = fields[0];
                this.notes = fields[1];
                this.type = fields[2];
                this.quantity = int.Parse(fields[3]);
                this.url = fields[4];
            }
        }

        public void FillFields(GHPart p)
        {
            this.text = Guide.UnescapeHtml(p.text);
            this.notes = Guide.UnescapeHtml(p.notes);
            this.type = Guide.UnescapeHtml(p.type);
            this.quantity = int.Parse(p.quantity);
            this.url = Guide.UnescapeHtml(p.url);
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
            return this.text + DELIM +
                   this.notes + DELIM +
                   this.type + DELIM +
                   this.quantity + DELIM +
                   this.url;
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
