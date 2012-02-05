using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;

using System.Diagnostics;

namespace iFixit7
{
    public class iFixitJSONHelper
    {
        private static string IFIXIT_API_AREAS = "http://www.ifixit.com/api/0.1/areas";
        private static string IFIXIT_API_GUIDES = ("http://www.ifixit.com/api/0.1/areas");
        private static string jsonResponse;
        private static List<Node> mTree;

//        public delegate void AreaCallEventHandler(iFixitJSONHelper sender, List<Node> tree);
//        public static event AreaCallEventHandler callAreasAPI;


//        public static List<Node> getAreas() {
//            callAreasAPI += new AreaCallEventHandler(iFixitJSONHelper_callAreasAPI);
//            doAPICallAsync(IFIXIT_API_AREAS);
//            return mTree;
//        }

//        void iFixitJSONHelper_callAreasAPI(iFixitJSONHelper sender, List<Node> tree)
//        {
            // update the UI
//            throw new NotImplementedException();
//        }

        public static void doAPICallAsync(string uri) {
            Uri site = new Uri(uri);

            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(site, stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);

            stream.Close();
        }

        private static void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            using (var reader = new StreamReader(e.Result))
            {
                jsonResponse = reader.ReadToEnd();
                JObject jo = JObject.Parse(jsonResponse);

                IEnumerable<JProperty> props = jo.Properties();
                foreach (JProperty p in props) {
                    Debug.WriteLine(p.Name);
                    if (!p.HasValues)
                        break;
                    IJEnumerable<JToken> values = p.Values();
                    foreach (JToken t in values)
                    {
                        decipherAreasJSON(t);
                    }
                }
                callAreasAPI.Invoke(null, mTree);
            }
            
        }

        private static void decipherAreasJSON(JToken jt)
        {
            if (jt is JProperty)
            {
                if (jt.ToObject<JProperty>().Name != "DEVICES")
                {
//                    Debug.WriteLine(" " + jt.ToObject<JProperty>().Name);
                    mTree.Add(new Node(jt.ToObject<JProperty>().Name, new List<Object>()));
                    if (jt.HasValues)
                    {
                        IJEnumerable<JToken> values = jt.Values();
                        foreach (JToken val in values)
                        {
                            decipherAreasJSON(val);
                        }
                    }
                }
                else
                {
                    IJEnumerable<JToken> devs = jt.Values();
                    foreach (JToken dev in devs)
                    {
                        mTree.Add(new Node(dev.ToString()));
//                        Debug.WriteLine("  " + dev.ToString());
                    }
                }
            }
            else
            {
                // can currently safely ignore anything in this category
//                Debug.WriteLine("\n" + jt.ToString() + " not a property\n");
            }
        }
    }
}
