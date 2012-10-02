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
        public static string IFIXIT_CATEGORY_OBJECT_KEY = "CATEGORIES";
        private static bool categories;
        private static string jsonResponse;
        //private static Node mTree = new Node("root", new List<Node>());
        private static Category mRootGroup = new Category();

        public delegate void AreaCallEventHandler(MainPage sender, Category tree);
        public event AreaCallEventHandler callAreasAPI;

        public void doAPICallAsync(string uri) {
            Uri site = new Uri(uri);
            categories = uri == IFIXIT_API_CATEGORIES;

            WebClient client = new WebClient();
            MemoryStream stream = new MemoryStream();
            client.OpenReadAsync(site, stream);
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);

            stream.Close();
        }

        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            using (var reader = new StreamReader(e.Result))
            {
                jsonResponse = reader.ReadToEnd();
                JObject jo = JObject.Parse(jsonResponse);
                //Debug.WriteLine(jo.ToString());
                if (categories) {
                    IEnumerable<JProperty> props = jo.Properties();
                    foreach (JProperty p in props)
                    {
                        //Debug.WriteLine(p.Name);
                        // You found a leaf node
                        if (p.Name.Equals(IFIXIT_CATEGORY_OBJECT_KEY))
                        {
                            break;
                        }
                        //Node curr = new Node(p.Name, new List<Node>());
                        Category curr = new Category();
                        curr.Name = p.Name;
                        //FIXME
                        //curr.Categories = new List<Category>();

                        mRootGroup.Categories.Add(curr);
                        //mTree.getChildrenList().Add(curr);
                        if (!p.HasValues)
                            break;
                        IJEnumerable<JToken> values = p.Values();
                        foreach (JToken t in values)
                        {
                            decipherAreasJSON(t, curr);
                        }
                    }
                } else {
                    IEnumerable<JProperty> props = jo.Properties();
                    foreach (JProperty p in props)
                    {
                        if (p.Name.Equals("guide"))
                        {
                            IJEnumerable<JToken> values = p.Values();
                            foreach (JToken t in values)
                            {
                                if (t is JProperty) {
                                    if (t.ToObject<JProperty>().Name.Equals("steps")) {
                                        //Debug.WriteLine(t.ToString());
                                        IJEnumerable<JToken> steps = t.Values();
                                        foreach (JToken step in steps)
                                        {
                                            Debug.WriteLine(step.ToString());
                                            StepTemp st = new StepTemp();
                                            //if (step is JProperty) {
                                            IEnumerable<JProperty> stepProps = step.ToObject<JObject>().Properties();
                                            foreach (JToken elem in stepProps)
                                            {
                                                if (elem.ToObject<JProperty>().Name.Equals("images"))
                                                {
                                                    //Debug.WriteLine(t.ToString());
                                                    jsonImage myImage = new jsonImage();
                                                    IJEnumerable<JToken> imageVals = elem.Values();
                                                    foreach (JToken val in imageVals) {
                                                        if (val.ToObject<JProperty>().Name.Equals("imageid")) {
                                                            myImage.setImageId(int.Parse(val.ToString()));
                                                            Debug.WriteLine("Just set myImage's id to " + val.ToString());
                                                        }
                                                        else if (val.ToObject<JProperty>().Name.Equals("text"))
                                                        {
                                                            myImage.setText(val.ToString());
                                                            Debug.WriteLine("Just set myImage's text to " + val.ToString());
                                                        }
                                                    }
                                                    // FIXME TODO
                                                    st.setImage(0, myImage);
                                                }
                                                else if (elem.ToObject<JProperty>().Name.Equals("lines"))
                                                {
                                                    IJEnumerable<JToken> lineVals = elem.Values();
                                                    jsonLines myLines = new jsonLines();
                                                    foreach (JToken val in lineVals) {
                                                        if (val.ToObject<JProperty>().Name.Equals("text"))
                                                        {
                                                            myLines.setText(val.ToString());
                                                        }
                                                    }
                                                    st.setLines(0, myLines);
                                                }
                                                else if (elem.ToObject<JProperty>().Name.Equals("number"))
                                                {
                                                    st.setStepNum(int.Parse(elem.ToString()));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                this.callAreasAPI.Invoke(null, mRootGroup);
            }
            
        }

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
                    Category curr = new Category();
                    curr.Name = jt.ToObject<JProperty>().Name;
                    //FIXME
                    //curr.Categories = new List<Category>();
                    
                    group.Categories.Add(curr);
                    //node.getChildrenList().Add(curr);
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
                        //group.Devices.Add(d);
                        group.Topics.Add(d);
                        
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
