using System;
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

namespace iFixit7
{
    public partial class Guide : PhoneApplicationPage
    {
        string guideTitle;
        int guideID;

        public Guide()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");

            //get the device name that was passed and stash it
            this.guideTitle = this.NavigationContext.QueryString["guideTitle"];
            this.guideID = int.Parse(this.NavigationContext.QueryString["guideID"]);
            GuideTitle.Text = this.guideTitle;

            //api call. The callback will be fired to populate the view
            JSONInterface2 ji = new JSONInterface2();
            ji.populateGuideView(this.guideID, populateGuideUI);
        }
        public bool populateGuideUI(GuideHolder guide){
            //now take the data from the object and populate the view!
            ListBoxItem pad = null;

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
            infoT.TextDecorations = TextDecorations.Underline;
            infoLB.Items.Add(infoT);

            //summary
            infoT = new TextBlock();
            infoT.Text = guide.guide.summary;
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
            pad.Padding = new Thickness(5);
            infoLB.Items.Add(pad);

            infoT = new TextBlock();
            infoT.Text = "Author: " + guide.guide.author.text;
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
                    i.Source = new BitmapImage(new Uri(img.text));

                    lbPadding = new ListBoxItem();
                    lbPadding.Padding = new Thickness(5);

                    lb.Items.Add(i);
                    lb.Items.Add(lbPadding);
                }

                this.GuidePivot.Items.Add(pi);
            }

            return true;
        }
    }
}
    /*
     
    public class GuideHolder
    {
        public string guideTitle { get; set; }
        public int guideID { get; set; }
        public LinkedList<StepTemp> steps { get; set; }

        public GuideHolder()
        {
            steps = new LinkedList<StepTemp>();
        }

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
        //steps
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
*/