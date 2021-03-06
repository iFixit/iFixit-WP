﻿using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;

namespace iFixit7
{
    /*
     * The classes to hold unpacked JSON search results
     */
    public class SRHolder
    {
        //holds a string of the query
        public string search { get; set; }

        //holds an array of the search results
        public SRResult[] results { get; set; }
    }
    public class SRResult
    {
        public string name { get; set; }
        public string title { get; set; }
        public string display_title { get; set; }
        public string summary { get; set; }

        //a truncated version of the summary
        public string shortSummary
        {
            get
            {
                if (summary != null)
                {
                    if (summary.Length > 150)
                        return string.Concat(summary.Remove(150), "...");
                    return summary;
                }
                return summary;
            }
            set
            {
                shortSummary = "";
            }
        }
        public string id { get; set; }

        public string guid { get; set; }

        //images
        public string mini { get; set; }
        public string thumbnail { get; set; }
        public string standard { get; set; }
        public string medium { get; set; }

        //there is a bit more info, but I do not think we care
    }

    public class Search
    {
        public static string IFIXIT_API_SEARCH = "http://www.ifixit.com/api/1.0/search/";

        /*
         * Makes an async web request for the search query, then scrapes the HTML result and returns a list
         * of hits
         */
        //See: http://www.codethinked.com/c-closures-explained
        public static void SearchForString(string query, Func<List<SRResult>, int> resultHook)
        {
            //can pass the API endpoint different params to filter results. For nor force it to return 'devices',
            //which seem fit for DeviceInfo pages
            WebRequest wr = WebRequest.Create(new Uri(IFIXIT_API_SEARCH + Uri.EscapeUriString(query) + "?filter=device"));

            //make the asynchronous request
            wr.BeginGetResponse(delegate(IAsyncResult requestResult)
            {
                //List<SRResult> Results = new List<SRResult>();

                WebRequest myRequest = (HttpWebRequest)requestResult.AsyncState;
                WebResponse wresponse = (WebResponse)myRequest.EndGetResponse(requestResult);
                string rawJSON = new StreamReader(wresponse.GetResponseStream()).ReadToEnd();

                //unpack the JSON into objects
                SRHolder sr = JsonConvert.DeserializeObject<SRHolder>(rawJSON);

                //resultHook(sr.results.ToList());

                List<SRResult> outList = new List<SRResult>();

                foreach (SRResult r in sr.results)
                {
                    Debug.WriteLine(r.title + ": " + r.guid);
                    if (r.guid != null)
                        outList.Add(r);
                }

                resultHook(outList);

                //actually runs to here
                return;
            }, wr);
        }
    }
}
