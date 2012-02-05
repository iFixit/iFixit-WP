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
        GuideHolder ourGuide = null;

        public Guide()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            Debug.WriteLine("A Guide has been navigated to. We are going to make API calls and do SCIENCE");

            ourGuide = new GuideHolder();

            //get the device name that was passed and stash it
            ourGuide.deviceName = this.NavigationContext.QueryString["device"];
            DeviceName.Text = ourGuide.deviceName;

            //API call to get the entire contents of the guide

            //parse it, and get the resulting GuideHolder

            //add stuff from the API call to populate the view

            //FIXME HACK. REMOVE THIS
            GuideStep g = null;
            for (int i = 1; i <= 4; i++)
            {
                g = new GuideStep(i);
                ourGuide.steps.AddLast(g);

                g.index = i;
                g.instructions = "Use a coin to rotate the battery-locking screw 90 degrees clockwise.";

                g.images.AddLast(new Uri("http://www.ifixit.com/igi/dLF6KygThyYNdyCS", UriKind.Absolute));
                g.images.AddLast(new Uri("http://www.ifixit.com/igi/FN4ThHdXJENYggv1", UriKind.Absolute));
            }

            PivotItem pi = null;
            ListBox lb = null;
            //foreach across all steps gs
            foreach (GuideStep gs in ourGuide.steps)
            {
                pi = new PivotItem();
                pi.Header = "Step " + gs.index;

                //add a grid to put the guide in
                lb = new ListBox();
                pi.Content = lb;

                //fill in the grid
                TextBlock tb = new TextBlock();
                tb.MaxWidth = 480 - 30;
                tb.TextWrapping = TextWrapping.Wrap;
                tb.Text = gs.instructions;
                tb.Padding = new Thickness(0, 5, 0, 9);

                lb.Items.Add(tb);

                Image i = null;
                ListBoxItem lbPadding = null;
                foreach (Uri img in gs.images)
                {
                    //load the image into i, then add it to the grid
                    i = new Image();
                    i.Source = new BitmapImage(img);

                    lbPadding = new ListBoxItem();
                    lbPadding.Padding = new Thickness(0, 5, 0, 5);
                    
                    lb.Items.Add(i);
                }

                GuidePivot.Items.Add(pi);
            }
        }
    }
    
    /* holds an entire guide */
    class GuideHolder
    {
        public string deviceName { get; set; }
        public LinkedList<GuideStep> steps { get; set; }

        public GuideHolder()
        {
            steps = new LinkedList<GuideStep>();
        }
    }

    /* Holds individual steps */
    class GuideStep
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