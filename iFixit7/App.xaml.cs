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
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Windows.Threading;
using System.Net.NetworkInformation;

namespace iFixit7
{
    public partial class App : Application, INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Specify the local database connection string.
        public const string DBConnectionString = "Data Source=isostore:/iFixit.sdf";

        //some constants
        public const string RootCategoryName = "root";
        public const string MagicParentTag = "parent";
        public const string MagicSelectedTag = "SelectedCategory";
        public const string MagicTypeTag = "SelectedType";

        private static int thumbCount = 0;

        public Category root { get; set; }

        private iFixitJSONHelper ifj;


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
            root = null;

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

            //make sure the folder of images for the local cache exists
            IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                if (!iso.DirectoryExists(ImgCache.BASE_PATH))
                {
                    iso.CreateDirectory(ImgCache.BASE_PATH);
                }
            }
            catch (Exception ex)
            { }

            // POST LOADING SCREEN

            //this has asynchroneous components that will run while the UI loads and whatnot
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                getAreas();
            }
            else
            {
                using (iFixitDataContext tDB = new iFixitDataContext(DBConnectionString))
                {
                    if (tDB.DatabaseExists() == true)
                    {
                        root = GetRootCategoryFromDB(tDB);
                    }
                }
                // clear loading screen
                (RootFrame.Content as MainPage).StopLoadingIndication();


            }
            // Create the database if it does not exist.
            //the using statement guarntees that setup and teardown will always happen, and properly
            using (iFixitDataContext tDB = new iFixitDataContext(DBConnectionString))
            {
                // Create the local database.
                if (!tDB.DatabaseExists())
                {
                    tDB.CreateDatabase();

                    // Save categories to the database.
                    tDB.SubmitChanges();
                }
            }
        }

        private Category GetRootCategoryFromDB(iFixitDataContext dc)
        {
            return BuildSubTree(DBHelpers.GetCompleteCategory("root", dc), dc);
        }

        private Category BuildSubTree(Category category, iFixitDataContext dc)
        {
            List<Category> newCats = new List<Category>();
            foreach (Category c in category.Categories)
            {
                newCats.Add(BuildSubTree(DBHelpers.GetCompleteCategory(c.Name, dc), dc));
            }
            category.Categories = newCats;
            return category;
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
            var _Worker = new BackgroundWorker();
            _Worker.DoWork += (s, e) =>
            {
                if (tree == null)
                {
                    //FIXME not sure this is what we really want to do...
                    return;
                }

                Debug.WriteLine("we got a tree, right? PROCESS IT");

                using (iFixitDataContext db = new iFixitDataContext(DBConnectionString))
                {
                    //add the rest of the tree
                    Debug.WriteLine("about to insert tree");
                    DBHelpers.InsertTree(tree, db);
                    db.SubmitChanges();
                    root = GetRootCategoryFromDB(db);
                    Debug.WriteLine("done inserting tree");

                    //now async get images for all root categories
                    //List<Category> cats = DBHelpers.GetCompleteCategory("root", db).Categories;
                    List<Category> cats = root.Categories;
                    thumbCount = 0;
                    foreach (Category c in cats)
                    {
                        Debug.WriteLine("thumb = " + c.Thumbnail);
                        if (c.Thumbnail == "")
                        {
                            ++thumbCount;
                            new JSONInterface2().populateDeviceInfo(c.Name, storeJSONCategoryInDB);
                        }
                    }
                }

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MainPage m = RootFrame.Content as MainPage;
                    if (m != null)
                        m.initDataBinding();
                });
            };
            _Worker.RunWorkerCompleted += (s, e) =>
            {
                if (thumbCount == 0)
                    (RootFrame.Content as MainPage).StopLoadingIndication();
            };
            _Worker.RunWorkerAsync();
        }

        /*
         * Take in the results from the API query to get additional info about categories (IE thumb) and
         * add it to the DB entries
         */
        bool storeJSONCategoryInDB(DeviceInfoHolder di)
        {
            using (iFixitDataContext db = new iFixitDataContext(App.DBConnectionString))
            {
                Category c = db.CategoriesTable.Single(cat => cat.Name == di.title);
                Debug.WriteLine("Category: " + c.Name + "| " + c.parentName + "| ");

                if (c.Thumbnail != di.image.text + ".standard")
                {
                    Debug.WriteLine("updating thumb");

                    NotifyPropertyChanging("Category.Thumbnail");
                    Debug.WriteLine("thumbnail before assign: " + c.Thumbnail);
                    c.Thumbnail = di.image.text + ".standard";
                    Debug.WriteLine("thumbnail after assign: " + c.Thumbnail);

                    //update the DB
                    db.SubmitChanges();
                    NotifyPropertyChanged("Category.Thumbnail");

                    --thumbCount;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            //manually force the cache to save the image
                            //ImgCache.RetrieveAndCacheByURL(di.image.text);

                            if (thumbCount == 0)
                            {

                                //update the root categories so the handle has thumbnails
                                using (iFixitDataContext tdb = new iFixitDataContext(App.DBConnectionString))
                                {
                                    for (int i = 0; i < root.Categories.Count; i++)
                                    {
                                        Category roots = root.Categories.ElementAt(i);
                                        Category newCat = tdb.CategoriesTable.FirstOrDefault(r => r.Name == roots.Name);

                                        roots.Thumbnail = newCat.Thumbnail;
                                    }
                                }
                                (RootFrame.Content as MainPage).initDataBinding();
                                (RootFrame.Content as MainPage).StopLoadingIndication();

                            }
                        });
                }
            }

            return true;
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
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