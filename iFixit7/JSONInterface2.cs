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

namespace iFixit7
{
    /*
     * Wraps around API calls. Allows the app to ask for a particular thing and get it back without
     * fussing with API stuff.
     */
    public class JSONInterface2
    {
        public static string IFIXIT_API_AREAS = "http://www.ifixit.com/api/0.1/areas";
        public static string IFIXIT_API_GUIDES = ("http://www.ifixit.com/api/0.1/guide/");          //takes guide id
        public static string IFIXIT_API_DEVICE_INFO = ("http://www.ifixit.com/api/0.1/device/");    //takes string name

        private Func<DeviceInfoHolder, bool> devInfoCallback = null;

        public void populateDeviceInfo(string dev, Func<DeviceInfoHolder, bool> f)
        {
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
            //this strea, has the string: e.Result. Read it out
            string res = new StreamReader(e.Result).ReadToEnd();

            //Console.WriteLine("inside completed. Got [[ " + res);

            res = Uri.UnescapeDataString(res);
            DeviceInfoHolder di = JsonConvert.DeserializeObject<DeviceInfoHolder>(res);

            this.devInfoCallback(di);
        }





        public static GuideHolder getGuide(int guideID)
        {
            return null;
        }
    }
    /*
     * Holds the result from a query for device info. There are a number of classes needed to make up
     * a single device info model.
     */
    public class DeviceInfoHolder
    {
        //public string[] categories { get; set; }
        public string contents { get; set; }
        public string description { get; set; }
        public DIDevInfo device_info { get; set; }

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
        public string guideTitle { get; set; }
        public int guideID { get; set; }
        public LinkedList<StepTemp> steps { get; set; }

        public GuideHolder()
        {
            steps = new LinkedList<StepTemp>();
        }

        /*
         * Takes all the data encoded in this GuideHolder and pours it into the passed in pivot
         */
        public void guideToView(Pivot parent)
        {
            if (parent == null)
                return;

            PivotItem pi = null;
            ListBox lb = null;
            //foreach across all steps gs
            foreach (StepTemp gs in this.steps)
            {
                pi = new PivotItem();
                pi.Header = "Step " + gs.getStepNum();

                //add a grid to put the guide in
                lb = new ListBox();
                pi.Content = lb;

                //fill in the grid
                TextBlock tb = new TextBlock();
                tb.MaxWidth = 480 - 30;
                tb.TextWrapping = TextWrapping.Wrap;
                // TODO FIXME: this is hardcoded to 0
                tb.Text = gs.getLines(0).getText();
                // TODO also use other methods
                tb.Padding = new Thickness(0, 5, 0, 9);

                lb.Items.Add(tb);

                Image i = null;
                ListBoxItem lbPadding = null;
                foreach (jsonImage img in gs.getImageList())
                {
                    //load the image into i, then add it to the grid
                    i = new Image();
                    i.Source = new BitmapImage(new Uri(img.getText()));

                    lbPadding = new ListBoxItem();
                    lbPadding.Padding = new Thickness(0, 5, 0, 5);

                    lb.Items.Add(i);
                }

                parent.Items.Add(pi);
            }
        }
        /* Holds individual steps for a single guide */
        private class GuideStep
        {
            //step #
            public int index { get; set; }

            //instructions for that step
            public string instructions { get; set; }

            //any images (as Uris)
            public LinkedList<Uri> images { get; set; }

            public GuideStep(int dex)
            {
                this.index = dex;
                images = new LinkedList<Uri>();
            }
        }
    }
    
    /*
     * Different ways of wrapping the web request
     */
    /*
    public class WebRequestExtensions
    {
        private static AutoResetEvent _event = null;
        private string resString;

        public string getReqResponse(string url)
        {
            _event = new AutoResetEvent(false);

            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(new Uri(url), stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(ReadCompleted);
            stream.Close();

            Console.WriteLine("about to block for finish web req");

            // block until async call is complete
            //_event.WaitOne();
            _event.WaitOne();

            return resString;
        }
        private void ReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            //this strea, has the string: e.Result. Read it out
            resString = new StreamReader(e.Result).ReadToEnd();

            Console.WriteLine("inside completed. Got [[ " + resString);

            //JsonConvert.DeserializeObject<DeviceInfoHolder>(response);
        }

        /*
        public static string HackedHttpGet(string url)
        {
            HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
            string result = null;
            //using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            using (HttpWebResponse resp = req.g as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }
            return result;
        }
        public static WebResponse GetResponse(this WebRequest request)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            IAsyncResult asyncResult = request.BeginGetResponse(r => autoResetEvent.Set(), null);

            // Wait until the call is finished
            autoResetEvent.WaitOne();

            return request.EndGetResponse(asyncResult);
        }

        public static Stream GetRequestStream(this WebRequest request)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);

            IAsyncResult asyncResult = request.BeginGetRequestStream(r => autoResetEvent.Set(), null);

            // Wait until the call is finished
            autoResetEvent.WaitOne();

            return request.EndGetRequestStream(asyncResult);
        }
    }
    */
}
