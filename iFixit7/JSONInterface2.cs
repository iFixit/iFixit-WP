/*
 * A second attempt at making the JSON stuff work...
 */
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
using System.Threading;
using System.IO;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Text;
using System.Reflection;

namespace iFixit7
{
    /*
     * Wraps around API calls. Allows the app to ask for a particular thing and get it back without
     * fussing with API stuff.
     */
    public class JSONInterface2
    {
        public static string IFIXIT_API_AREAS = "http://www.ifixit.com/api/1.0/categories/";
        public static string IFIXIT_API_GUIDES = ("http://www.ifixit.com/api/1.0/guide/");          //takes guide id
        public static string IFIXIT_API_DEVICE_INFO = ("http://www.ifixit.com/api/1.0/topic/");    //takes string name

        private Func<DeviceInfoHolder, bool> devInfoCallback = null;
        private Func<GuideHolder, bool> guidePopulateCallback = null;

        /*
         * Called from DeviceInfo.xaml to retreive data and fill in its ui
         */
        public void populateDeviceInfo(string dev, Func<DeviceInfoHolder, bool> f)
        {
            //save the callback for when the asynchroneous operation returns
            devInfoCallback = f;

            //request
            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(new Uri(IFIXIT_API_DEVICE_INFO + Uri.EscapeUriString(dev)), stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(GetDeviceInfoCompleted);
            stream.Close();
        }

        private void GetDeviceInfoCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            //e.Result as the web request result. Read it out
            string rawJSON = new StreamReader(e.Result).ReadToEnd();

            //rawJSON = Uri.UnescapeDataString(rawJSON);
            DeviceInfoHolder di = JsonConvert.DeserializeObject<DeviceInfoHolder>(rawJSON);

            devInfoCallback(di);
        }


        /*
         * Called from some part of the guide xaml to retreive data and fill in its ui
         */
        public void populateGuideView(int guideID, Func<GuideHolder, bool> f)
        {
            //save the callback for when the asynchroneous operation returns
            guidePopulateCallback = f;

            //request
            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(new Uri(IFIXIT_API_GUIDES + Uri.EscapeUriString(guideID.ToString())), stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(GetGuideCompleted);
            stream.Close();
        }
        private void GetGuideCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            string rawJSON = new StreamReader(e.Result).ReadToEnd();

            //rawJSON = Uri.UnescapeDataString(rawJSON);
            GuideHolder gde = JsonConvert.DeserializeObject<GuideHolder>(rawJSON);


            //apply the URL unescaper and HTML stripper to all strings in the class
            /*
            var fields = typeof(GuideHolder).GetFields(System.Reflection.BindingFlags.GetField);
            foreach (FieldInfo f in fields) {
                var val = f.GetValue(gde);
                if (val != null)
                {
                    if(val is string)
                    {
                        string str = (string)val;
                        str = Uri.UnescapeDataString(str);
                        str = StripTagsCharArray(str);
                        val = str;
                    }
                }
            }
            */
            //this DOES WORK. it just needs to be called on everything...
            //gde.guide.steps[0].lines[4].text = StripTagsCharArray(gde.guide.steps[0].lines[4].text);

            guidePopulateCallback(gde);
        }
    }

    /*
     * 
     */

    /*
     * Holds the result from a query for topic info. There are a number of classes needed to make up
     * a single device info model.
     */
    public class DeviceInfoHolder
    {
        //public string[] categories { get; set; }
        public string contents { get; set; }
        public string description { get; set; }
        public DIDevInfo topic_info { get; set; }

        //public string[] flags { get; set; }

        public DIGuides[] guides { get; set; }

        public DIImage image { get; set; }

        public string locale { get; set; }
        public DIParts parts { get; set; }
        public DISolutions solutions { get; set; }
        public string title { get; set; }
        //string[] tools { get; set; }
    }
    public class DIDevInfo
    {
        public string discontinued { get; set; }
        public string introduced { get; set; }
        public string manufacturer { get; set; }
        public string name { get; set; }
    }
    public class DIGuides
    {
        public string guideid { get; set; }
        public string subject { get; set; }
        public string thumbnail { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string url { get; set; }
    }
    public class DIImage
    {
        public string text { get; set; }
    }
    public class DIParts
    {
        public string partCategory { get; set; }
        public string url { get; set; }
    }
    public class DISolutions
    {
        public string count { get; set; }
        public string url { get; set; }
    }


    /*
     * holds an entire guide
     */
    public class GuideHolder
    {
        public string device { get; set; }
        public GHGuide guide { get; set; }
        public string guideid { get; set; }
        public string url { get; set; }
    }
    public class GHGuide
    {
        public GHAuthor author { get; set; }

        public string[] categories { get; set; }
        public string conclusion { get; set; }

        //public string device { get; set; }
        public string difficulty { get; set; }

        //public string[] documents { get; set; }
        //public string[] flags { get; set; }

        public string guideid { get; set; }
        public GHImage image { get; set; }

        public string introduction { get; set; }
        public string introduction_rendered { get; set; }
        public string locale { get; set; }

        public GHPart[] parts { get; set; }

        public GHPrereq[] prereqs { get; set; }

        public GHStep[] steps { get; set; }

        public string subject { get; set; }
        public string summary { get; set; }     //drop this? null?
        public string time_required { get; set; }
        public string title { get; set; }

        public GHTool[] tools { get; set; }

        public string type { get; set; }
    }
    public class GHAuthor
    {
        public string text { get; set; }
        public string userid { get; set; }
    }
    public class GHImage
    {
        public string imageid { get; set; }
        public string text { get; set; }
    }
    public class GHPart
    {
        public string notes { get; set; }
        public string text { get; set; }
        public string thumbnail { get; set; }
        public string url { get; set; }
    }
    public class GHPrereq
    {
        public string guideid { get; set; }
        public string locale { get; set; }
        public string text { get; set; }
    }
    public class GHTool
    {
        public string notes { get; set; }
        public string text { get; set; }
        public string thumbnail { get; set; }
        public string url { get; set; }
    }

    //a step and its components
    public class GHStep
    {
        public GHStepImage[] images { get; set; }
        public GHStepLines[] lines { get; set; }

        public string number { get; set; }
        public string title { get; set; }
    }
    public class GHStepImage
    {
        public string imageid { get; set; }
        public string orderby { get; set; }
        public string text { get; set; }
    }
    public class GHStepLines
    {
        public string bullet { get; set; }
        public string level { get; set; }
        public string text { get; set; }
    }
}
