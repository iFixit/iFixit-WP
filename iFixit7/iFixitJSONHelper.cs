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
        // Use the 1.0 api
        public static string IFIXIT_API_CATEGORIES = "http://www.ifixit.com/api/1.0/categories";
        // public static string IFIXIT_API_AREAS = "http://www.ifixit.com/api/0.1/areas";
        public static string IFIXIT_API_GUIDES = ("http://www.ifixit.com/api/0.1/guide/");
        public static string IFIXIT_CATEGORY_OBJECT_KEY = "TOPICS";
        private static bool categories;
        private static string jsonResponse;

        private static Category mRootGroup = new Category("root");

        public delegate void AreaCallEventHandler( Category tree);
        public event AreaCallEventHandler callAreasAPI;

        public void doAPICallAsync(string uri)
        {
            Uri site = new Uri(uri);

            //FIXME this scares me... Do we need it if we only use this for categories now?
            categories = uri == IFIXIT_API_CATEGORIES;

            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(site, stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);

            stream.Close();
        }

        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                using (var reader = new StreamReader(e.Result))
                {
                    jsonResponse = reader.ReadToEnd();
                    JObject jo = JObject.Parse(jsonResponse);

                    //NO! This causes queries for 'root''s chilren to return 'root' as well
                    //mRootGroup.parentName = mRootGroup.Name;

                    //if we are processing a categories call (always yes now?)
                    if (categories)
                    {
                        IEnumerable<JProperty> props = jo.Properties();
                        foreach (JProperty p in props)
                        {
                            // You found a leaf node
                            if (p.Name.Equals(IFIXIT_CATEGORY_OBJECT_KEY))
                            {
                                break;
                            }
                            Category curr = new Category(p.Name);

                            mRootGroup.AddCategory(curr);
                            //mRootGroup.Categories.Add(curr);

                            if (!p.HasValues)
                                break;
                            IJEnumerable<JToken> values = p.Values();
                            foreach (JToken t in values)
                            {
                                decipherAreasJSON(t, curr);
                            }
                        }
                    }

                    this.callAreasAPI.Invoke(mRootGroup);
                }
            }
            catch (WebException we)
            {
                //there was an exception using the internet connection. Return null
                this.callAreasAPI.Invoke(null);
            }
            catch (Newtonsoft.Json.JsonReaderException je)
            {
                this.callAreasAPI.Invoke(null);
            }
            
        }

        /*
         * Gets all the sub-categories (or topics?) of jt, and hangs them under node?
         */
        //private static void decipherAreasJSON(JToken jt, Node node)
        private static void decipherAreasJSON(JToken jt, Category group)
        {
            if (jt is JProperty)
            {
                if (jt.ToObject<JProperty>().Name != IFIXIT_CATEGORY_OBJECT_KEY)
                {
//                    Debug.WriteLine(" " + jt.ToObject<JProperty>().Name);

                    // since it's not a device, it must be a group
                    //Node curr = new Node(jt.ToObject<JProperty>().Name, new List<Node>());
                    Category curr = new Category(jt.ToObject<JProperty>().Name);
                    
                    //FIXME this is where we add to the 1:M, right?
                    //group.Categories.Add(curr);
                    group.AddCategory(curr);
                    if (jt.HasValues)
                    {
                        IJEnumerable<JToken> values = jt.Values();
                        foreach (JToken val in values)
                        {
                            decipherAreasJSON(val, curr);
                        }
                    }
                }
                // these are devices
                else
                {
                    IJEnumerable<JToken> devs = jt.Values();
                    foreach (JToken dev in devs)
                    {
                        Topic d = new Topic();
                        d.Name = dev.ToString();
                        /*
                        if (group.Devices == null)
                        {
                            group.Devices = new List<Topic>();
                        }
                         */
                        //group.Topics.Add(d);
                        group.AddTopic(d);
                        
                        //node.getChildrenList().Add(new Node(dev.ToString(), null));
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
