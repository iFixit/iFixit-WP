using System.Collections.Generic;
using System.Windows.Media;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;

namespace iFixit7
{
    [Table]
    public class Step : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Step() : base()
        {}

        /*
         * This constructor is for making a new Step from a GHStep
         */
        public Step(GHStep st)
        {
            this.FillFields(st);
        }

        public void FillFields(GHStep st)
        {
            this.StepIndex = st.number;
            this.Title = st.title;

            //add images (in a strange way...)
            this.Image1 = "";
            this.Image2 = "";
            this.Image3 = "";

            if (st.media.image != null)
            {
                switch (st.media.image.Length)
                {
                    default:
                        //no images, or we have no idea
                        break;

                    case 1:
                        this.Image1 = st.media.image[0].text;
                        break;

                    case 2:
                        this.Image1 = st.media.image[0].text;
                        this.Image2 = st.media.image[1].text;
                        break;

                    case 3:
                        this.Image1 = st.media.image[0].text;
                        this.Image2 = st.media.image[1].text;
                        this.Image3 = st.media.image[2].text;
                        break;
                }
            }

            //add each line
            foreach (GHStepLines l in st.lines)
            {
                Lines dbLine = new Lines(l);
                dbLine.parentName = this.parentName + this.StepIndex;
                this.Lines.Add(dbLine);
            }
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column]
        public string parentName { get; set; }

        public List<Lines> Lines = new List<Lines>();

        public void AddCategory(Lines l)
        {
            l.parentName = this.Title;
            this.Lines.Add(l);
        }



        private string _title;
        [Column]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                if (_title != value)
                {
                    NotifyPropertyChanging("Title");
                    _title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }


        //because there can be up to 3 images, hard code that possability for now. Should this use the Image table?
        //what number step this is
        private string _image1;
        [Column]
        public string Image1
        {
            get
            {
                return _image1;
            }
            set
            {
                if (_image1 != value)
                {
                    NotifyPropertyChanging("Image1");
                    _image1 = value;
                    NotifyPropertyChanged("Image1");
                }
            }
        }
        private string _image2;
        [Column]
        public string Image2
        {
            get
            {
                return _image2;
            }
            set
            {
                if (_image2 != value)
                {
                    NotifyPropertyChanging("Image2");
                    _image2 = value;
                    NotifyPropertyChanged("Image2");
                }
            }
        }
        private string _image3;
        [Column]
        public string Image3
        {
            get
            {
                return _image3;
            }
            set
            {
                if (_image3 != value)
                {
                    NotifyPropertyChanging("Image3");
                    _image3 = value;
                    NotifyPropertyChanged("Image3");
                }
            }
        }
        
        //what number step this is
        private string _stepIndex;
        [Column]
        public string StepIndex
        {
            get
            {
                return _stepIndex;
            }
            set
            {
                if (_stepIndex != value)
                {
                    NotifyPropertyChanging("StepIndex");
                    _stepIndex = value;
                    NotifyPropertyChanged("StepIndex");
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
