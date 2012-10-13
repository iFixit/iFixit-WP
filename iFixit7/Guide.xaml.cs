﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using Microsoft.Phone;
using System.IO;

namespace iFixit7
{
    public partial class GuideView : PhoneApplicationPage
    {
        public Guide SourceGuide = null;

        string guideID = "";

        public GuideView()
        {
            InitializeComponent();

            this.DataContext = SourceGuide;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");

            //this.guideID = int.Parse(this.NavigationContext.QueryString["GuideID"]);
            this.guideID = this.NavigationContext.QueryString["GuideID"];

            Debug.WriteLine("\tgot guide id = " + guideID);

            SourceGuide = App.mDB.GuidesTable.SingleOrDefault(g => g.GuideID == this.guideID);
            if (SourceGuide != null)
            {
                Debug.WriteLine("\tgot guide title = " + SourceGuide.Title);
            }

            /*
            //get the device name that was passed and stash it
            this.guideTitle = this.NavigationContext.QueryString["guideTitle"];
            
            GuideTitle.Text = this.guideTitle;
            GuideTitle.TextWrapping = TextWrapping.Wrap;

            //api call. The callback will be fired to populate the view
            JSONInterface2 ji = new JSONInterface2();
            ji.populateGuideView(this.guideID, populateGuideUI);
             * */
        }
        public bool populateGuideUI(GuideHolder guide){
            return true;

            /*
            //now take the data from the object and populate the view!
            ListBoxItem pad = null;

            //============
            //initial guide info tab
            PivotItem infoTab = new PivotItem();
            infoTab.Header = "Info";
            this.GuidePivot.Items.Add(infoTab);
            ListBox infoLB = new ListBox();
            infoTab.Content = infoLB;

            //title
            TextBlock infoT = new TextBlock();
            infoT.Text = guide.guide.title;
            infoT.TextAlignment = TextAlignment.Center;
            infoT.FontWeight = FontWeights.Bold;
            infoT.TextWrapping = TextWrapping.Wrap;
            infoLB.Items.Add(infoT);

            //summary
            infoT = new TextBlock();
            infoT.Text = guide.guide.summary;
            infoT.TextWrapping = TextWrapping.Wrap;
            infoLB.Items.Add(infoT);

            pad = new ListBoxItem();
            pad.Padding = new Thickness(5);
            infoLB.Items.Add(pad);

            if (guide.guide.difficulty != "")
            {
                infoT = new TextBlock();
                infoT.Text = "Difficulty: " + guide.guide.difficulty;
                infoLB.Items.Add(infoT);

                pad = new ListBoxItem();
                pad.Padding = new Thickness(5);
                infoLB.Items.Add(pad);
            }

            infoT = new TextBlock();
            infoT.Text = "Steps: " + guide.guide.steps.Length.ToString();
            infoLB.Items.Add(infoT);

            pad = new ListBoxItem();
            pad.Padding = new Thickness(7);
            infoLB.Items.Add(pad);

            //add all listed tools
            if (guide.guide.tools.Length > 0)
            {
                infoT = new TextBlock();
                infoT.Text = "Tools Needed:";
                infoT.FontStyle = FontStyles.Italic;
                infoLB.Items.Add(infoT);
                foreach (GHTool t in guide.guide.tools)
                {
                    infoT = new TextBlock();
                    infoT.Text = "-" + t.text;
                    if (t.notes != "")
                        infoT.Text += " (" + t.notes + ")";
                    infoLB.Items.Add(infoT);
                }

                pad = new ListBoxItem();
                pad.Padding = new Thickness(7);
                infoLB.Items.Add(pad);
            }

            //add relevant parts
            if (guide.guide.parts.Length > 0)
            {
                infoT = new TextBlock();
                infoT.Text = "Relevant Parts:";
                infoT.FontStyle = FontStyles.Italic;
                infoLB.Items.Add(infoT);
                foreach (GHPart p in guide.guide.parts)
                {
                    infoT = new TextBlock();
                    infoT.Text = "-" + p.text;
                    if (p.notes != "")
                        infoT.Text += " (" + p.notes + ")";
                    infoLB.Items.Add(infoT);
                }

                pad = new ListBoxItem();
                pad.Padding = new Thickness(7);
                infoLB.Items.Add(pad);
            }


            infoT = new TextBlock();
            infoT.Text = "Author: " + guide.guide.author.text;
            infoT.TextWrapping = TextWrapping.Wrap;
            infoLB.Items.Add(infoT);

            pad = new ListBoxItem();
            pad.Padding = new Thickness(5);
            infoLB.Items.Add(pad);

            //button on bottom for opening the guide in the browser
            HyperlinkButton hb = new HyperlinkButton();
            hb.Content = "View this guide in a browser";
            hb.NavigateUri = new Uri(guide.url);
            hb.TargetName = "_blank";
            hb.Padding = new Thickness(8);
            infoLB.Items.Add(hb);
             */

            //============
            //prereqs and whatnot tab
            /*
            PivotItem backTab = new PivotItem();
            backTab.Header = "Info";
            this.GuidePivot.Items.Add(backTab);
            ListBox backLB = new ListBox();
            backTab.Content = backLB;
            */

            /*
            //============
            //generate a tab for each step
            PivotItem pi = null;
            ListBox lb = null;
            int stepNum = 1;
            foreach (GHStep step in guide.guide.steps)
            {
                pi = new PivotItem();
                pi.Header = "Step " + stepNum;
                stepNum++;

                //add a grid to put the guide in
                lb = new ListBox();
                pi.Content = lb;

                //fill in the grid
                TextBlock tb = null;
                foreach (GHStepLines line in step.lines){
                    tb = new TextBlock();
                    //tb.MaxWidth = 480 - 30;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.Text = line.text;
                    tb.Padding = new Thickness(0, 5, 0, 9);

                    lb.Items.Add(tb);
                }

                Image i = null;
                ListBoxItem lbPadding = null;
                foreach (GHStepImage img in step.images)
                {
                    //load the image into i, then add it to the grid
                    i = new Image();
                    //i.Width = 480;
                    i.Source = new BitmapImage(new Uri(img.text + ".standard"));

                    //the new stuff
                    //i = new Image();
                    //i.Source = new BitmapImage();
                    //(i.Source as BitmapImage).UriSource = new Uri(img.text);
                    //System.IO.StreamReader sr = new System.IO.StreamReader(img.text);
                    //i.Source = PictureDecoder.DecodeJpeg(null, 192, 256);

                    allImages.Add(i);

                    lbPadding = new ListBoxItem();
                    lbPadding.Padding = new Thickness(5);

                    lb.Items.Add(i);
                    lb.Items.Add(lbPadding);
                }

                this.GuidePivot.Items.Add(pi);
            }
            
            return true;
             */
        }
    }
}
