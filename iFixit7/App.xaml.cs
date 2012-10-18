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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Diagnostics;

namespace iFixit7
{
    public partial class App : Application
    {
        // Specify the local database connection string.
        public const string DBConnectionString = "Data Source=isostore:/iFixit.sdf";

        //some constants
        public const string RootCategoryName = "root";
        public const string MagicParentTag = "CategoryParent";
        public const string MagicSelectedTag = "SelectedCategory";
        public const string MagicTypeTag = "SelectedType";

        private iFixitJSONHelper ifj;
        public static iFixitDataContext mDB;


        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {

            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off to GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

            //this has asynchroneous components that will run while the UI loads and whatnot
            getAreas();

            // Create the database if it does not exist.
            //the using statement guarntees that setup and teardown will always happen, and properly
            using (iFixitDataContext tDB = new iFixitDataContext(DBConnectionString))
            {
                if (tDB.DatabaseExists() == true)
                {
                    //FIXME until we add code to remove duplicates, this is the easiest solution
                    tDB.DeleteDatabase();

                    //MessageBox.Show("Loading data for the first time. This may take a moment...");
                }

                // Create the local database.
                tDB.CreateDatabase();

                // Save categories to the database.
                tDB.SubmitChanges();
            }
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        public void getAreas()
        {
            ifj = new iFixitJSONHelper();

            Debug.WriteLine("about to get areas....");

            ifj.callAreasAPI += new iFixitJSONHelper.AreaCallEventHandler(App_callAreasAPI);
            ifj.doAPICallAsync(iFixitJSONHelper.IFIXIT_API_CATEGORIES);
        }

        //the callback hook that gets fired when the area hierarchy is finally retreived
        public void App_callAreasAPI(Category tree)
        {
            if (tree == null)
            {
                //FIXME not sure this is what we really want to do
                MessageBox.Show("No network connection. Please run this app when connected to the internet.");
                (RootFrame.Content as MainPage).StopLoadingIndication(true);

                return;
            }

            Debug.WriteLine("we got a tree, right? PROCESS IT");

            //open up a new DB connection for this transaction
            mDB = new iFixitDataContext(DBConnectionString);

            // Prepopulate the categories
            //FIXME probably need to do duplicate checking here?
            mDB.CategoriesTable.InsertOnSubmit(tree);

            // Save categories to the database.
            mDB.SubmitChanges();

            /*
            IQueryable<Category> query = from cats in mDB.CategoriesTable
                                            where cats.Name == "root"
                                            select cats;
            //FIXME this is kindof evil... The main app should not be able/have to do this
            ((MainPage)RootFrame.Content).CatagoryList.ItemsSource = query.FirstOrDefault().Categories;
                * */
            (RootFrame.Content as MainPage).initDataBinding();
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}