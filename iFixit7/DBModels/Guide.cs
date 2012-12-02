using System.Collections.Generic;

// databasey things
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using Microsoft.Phone.Net.NetworkInformation;
using System.Diagnostics;
using iFixit7.DBModels;
using System.Runtime.Serialization;
using System.IO;
using System;

namespace iFixit7
{
    [Table]
    public class Guide : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public Guide() : base()
        {}

        /*
         * This constructor is for making a new Guide from a GuideHolder
         */
        public Guide(GuideHolder gh)
        {
            //set this once
            this.Thumbnail = gh.guide.image.medium;

            this.FillFields(gh);
        }

        public void FillFields(GuideHolder gh){
            //fill in fields
            this.Title = gh.guide.title;
            this.Topic = gh.topic;
            this.Summary = gh.guide.summary;
            this.URL = gh.url;
            this.GuideID = gh.guideid;
            this.TitleImage = gh.guide.image.standard;

            this.Author = gh.guide.author.text;
            this.Introduction = gh.guide.introduction;
            this.Difficulty = gh.guide.difficulty;

            //do not change this after initial set
            //this.Thumbnail = gh.guide.image.medium;

            this.Steps.Clear();
            //copy over steps
            foreach (GHStep s in gh.guide.steps)
            {
                Step dbStep = new Step(s);
                dbStep.parentName = this.GuideID;
                this.Steps.Add(dbStep);
            }

            //this.Populated = true;

            foreach (GHPart p in gh.guide.parts)
            {
                Part dbPart = new Part(p);
                this.AddPart(dbPart);
            }

            foreach (GHPrereq p in gh.guide.prereqs)
            {
                Prereq dbPrereq = new Prereq(p);
                this.AddPrereq(dbPrereq);
            }

            foreach (GHTool t in gh.guide.tools)
            {
                Tool dbTool = new Tool(t);
                this.AddTool(dbTool);
            }
        }

        /*
         * Fills in the fields of this object from the incomplete DeviceInfo model of a guide
         */
        public void FillFieldsFromDeviceInfo(string top, DIGuides g)
        {
            this.Title = g.title;
            this.Topic = top;
            this.Summary = "";
            this.URL = g.url;
            this.GuideID = g.guideid;
            this.Thumbnail = g.thumbnail;

            //this.Populated = false;
        }

        //the primary key
        [Column(AutoSync = AutoSync.OnInsert, IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id { get; set; }

        [Column]
        public string Introduction { get; set; }

        [Column]
        public string Difficulty { get; set; }

        [Column]
        public string Author { get; set; }

        [Column]
        public string parentName { get; set; }

        public List<Step> Steps = new List<Step>();

        public void AddStep(Step s)
        {
            s.parentName = this.Title;
            this.Steps.Add(s);
        }

        public List<Part> parts = new List<Part>();
        [Column]
        public string Parts
        {
            get
            {
                return serializeList<Part>(parts);
            }
            set
            {
                string[] items = deserializeList<Part>(value);
                foreach (string serPart in items)
                {
                    Part p = new Part(serPart);
                    AddPart(p);
                }
            }
        }

        public void AddPart(Part p)
        {
            parts.Add(p);
        }

        public List<Prereq> prereqs = new List<Prereq>();
        [Column]
        public string Prereqs
        {
            get
            {
                return serializeList<Prereq>(prereqs);

            }
            set
            {
                string[] items = deserializeList<Prereq>(value);
                foreach (string part in items)
                {
                    Prereq p = new Prereq(part);
                    AddPrereq(p);
                }
            }
        }

        public void AddPrereq(Prereq p)
        {
            prereqs.Add(p);
        }

        public List<Tool> tools = new List<Tool>();
        [Column]
        public string Tools
        {
            get
            {
                return serializeList<Tool>(tools);
            }
            set
            {
                string[] items = deserializeList<Tool>(value);
                foreach (string tool in items)
                {
                    Tool t = new Tool(tool);
                    AddTool(t);
                }
            }
        }

        public void AddTool(Tool t)
        {
            tools.Add(t);
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

                    //strip HTML formatting the hard way http://www.theukwebdesigncompany.com/articles/entity-escape-characters.php
                    _title = UnescapeHtml(value);
                    NotifyPropertyChanged("Title");
                }
            }
        }

        /*
         * a hook to get the shortened version of the title for binding
         */
        public string ShortTitle
        {
            get
            {
                if (_title != null)
                {
                    //if _title.Length > XXX
                    //remove topic name (and leading space) from guide titles
                    return _title.Replace( _topic + " ", "");
                }
                return "";
            }
        }

        //this should probably be an entity ref or something, not just a string
        private string _topic;
        [Column]
        public string Topic
        {
            get
            {
                return _topic;
            }
            set
            {
                if (_topic != value)
                {
                    NotifyPropertyChanging("Topic");
                    _topic = value;
                    NotifyPropertyChanged("Topic");
                }
            }
        }

        private string _summary;
        [Column]
        public string Summary
        {
            get
            {
                return _summary;
            }
            set
            {
                if (_summary != value)
                {
                    NotifyPropertyChanging("Summary");
                    _summary = value;
                    NotifyPropertyChanged("Summary");
                }
            }
        }

        private string _url;
        [Column]
        public string URL
        {
            get
            {
                return _url;
            }
            set
            {
                if (_url != value)
                {
                    NotifyPropertyChanging("URL");
                    _url = value;
                    NotifyPropertyChanged("URL");
                }
            }
        }

        private string _guideID;
        [Column]
        public string GuideID
        {
            get
            {
                return _guideID;
            }
            set
            {
                if (_guideID != value)
                {
                    NotifyPropertyChanging("GuideID");
                    _guideID = value;
                    NotifyPropertyChanged("GuideID");
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

        /*
         * A hacky property to bind to to set opacity based on populated
         */
        public double PopulatedOpacity
        {
            get
            {
                if (_populated || DeviceNetworkInformation.IsNetworkAvailable)
                {
                    return 1.0;
                }
                return 0.5;
            }
        }

        //the mini/thumbnail photo [thumbnail]
        private string _thumbnail;
        [Column]
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

        //a larger title photo [standard] (small?)
        private string _titleImage;
        [Column]
        public string TitleImage
        {
            get
            {
                return _titleImage;
            }
            set
            {
                if (_titleImage != value)
                {
                    NotifyPropertyChanging("TitleImage");
                    _titleImage = value;
                    NotifyPropertyChanged("TitleImage");
                }
            }
        }
        

        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        public static string UnescapeHtml(string s)
        {
            return s.Replace("&quot;", "\"").Replace("&amp;", "&").Replace("&nbsp;", " ");
        }

        private static string serializeList<T>(List<T> l)
        {
            string s = "";
            foreach (T p in l)
            {
                s += p + ";";
            }
            return s;
        }

        private static string[] deserializeList<T>(string s)
        {
            string[] delim = { ";" };
            return s.Split(delim, System.StringSplitOptions.RemoveEmptyEntries);
            //List<T> list = new List<T>();
            //foreach (string item in items)
            //{
            //    if (T == typeof(Prereq))
            //    {
            //        p = new Prereq(item);
            //    }
            //    list.Add(p);
            //}
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
