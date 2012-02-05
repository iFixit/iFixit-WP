﻿using System;
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
        public static string IFIXIT_API_AREAS = "http://www.ifixit.com/api/0.1/areas";
        public static string IFIXIT_API_GUIDES = ("http://www.ifixit.com/api/0.1/guide/");
        private static bool areas;
        private static string jsonResponse;
        private static List<Node> mTree;

        public delegate void AreaCallEventHandler(MainPage sender, List<Node> tree);
        public event AreaCallEventHandler callAreasAPI;


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

        public void doAPICallAsync(string uri) {
            Uri site = new Uri(uri);
            areas = uri == IFIXIT_API_AREAS;

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
                if (areas) {
                    IEnumerable<JProperty> props = jo.Properties();
                    foreach (JProperty p in props)
                    {
                        //Debug.WriteLine(p.Name);
                        if (!p.HasValues)
                            break;
                        IJEnumerable<JToken> values = p.Values();
                        foreach (JToken t in values)
                        {
                            decipherAreasJSON(t);
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
                                                        if (val.ToObject<JObject>().TryGetValue("text"))
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
                this.callAreasAPI.Invoke(null, mTree);
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
                        mTree.(new Node(dev.ToString(), null));
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
