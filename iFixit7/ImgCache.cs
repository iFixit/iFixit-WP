using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;
using System.Windows.Data;
using System.Diagnostics;

namespace iFixit7
{
    public class ImgCache
    {
        public const string BASE_PATH = "imageCache";
        public const string IFIXIT_IMG_URL_BASE = "http://guide-images.ifixit.net/igi/";
        /*
         * Save img under the name guid
         */
        //http://windowsphonegeek.com/tips/All-about-WP7-Isolated-Storage---Read-and-Save-Images
        /*
         * A wrapper for getting an image from the web, then storing it
         */
        public static BitmapImage RetrieveAndCacheByURL(string url)
        {
            BitmapImage img = null;

            //get image from web and force it to begin downloading immediately
            img = new BitmapImage(new Uri(url, UriKind.Absolute));
            img.CreateOptions = BitmapCreateOptions.None;

            //actually save to isolated storage once the image finishes downloading
            img.ImageOpened += new EventHandler<RoutedEventArgs>(ImageSaveComplete);

            return img;
        }

        static void ImageSaveComplete(object sender, RoutedEventArgs e)
        {
            BitmapImage bm = sender as BitmapImage;

            string src = bm.UriSource.ToString();
            int lastSlash = src.LastIndexOf('/');
            string guid = src.Substring(lastSlash + 1);

            ImgCache.StoreImage(guid, bm);
            Debug.WriteLine("done loading and saving image from URL");
        }
        public static void StoreImage(string guid, BitmapImage img)
        {
            // Create a filename for file in isolated storage.
            String tempJPEG = BASE_PATH + "\\" + guid + ".jpeg";

            // Create virtual store and file stream. Check for duplicate files
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.DirectoryExists(BASE_PATH))
                    myIsolatedStorage.CreateDirectory(BASE_PATH);

                if (myIsolatedStorage.FileExists(tempJPEG))
                {
                    myIsolatedStorage.DeleteFile(tempJPEG);
                }

                IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile(tempJPEG);

                /*
                StreamResourceInfo sri = null;
                Uri uri = new Uri(tempJPEG, UriKind.Relative);
                sri = Application.GetResourceStream(uri);

                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(sri.Stream); 
                WriteableBitmap wb = new WriteableBitmap(bitmap);
                 */
                WriteableBitmap wb = new WriteableBitmap(img);

                // Encode WriteableBitmap object to a JPEG stream.
                Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);

                //commented in example...
                //wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                fileStream.Close();
            }
        }



        /*
         * A wrapper for retreival by raw URL
         */
        public static BitmapImage GetImageByURL(string url)
        {
            int lastSlash = url.LastIndexOf('/');
            string guid = url.Substring(lastSlash + 1);

            return GetImage(guid);
        }
        public static BitmapImage GetImage(string guidWithSize)
        {
            BitmapImage bi = new BitmapImage();
            string fullPath = BASE_PATH + "\\" + guidWithSize + ".jpeg";

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if(!iso.FileExists(fullPath))
                {
                    //if we cannot find the image, return null
                    return null;
                }
                using (IsolatedStorageFileStream fileStream = iso.OpenFile(fullPath, FileMode.Open, FileAccess.Read))
                {
                    bi.SetSource(fileStream);

                    //FIXME need to set the URI source! else we cannot open it later for fullscreen view
                    bi.UriSource = new Uri(IFIXIT_IMG_URL_BASE + guidWithSize, UriKind.Absolute);

                    return bi;
                }
            }

            //if we cannot find the image, or opening iso storage fails
            return null;
        }
    }


    /*
     * This converter lets us transparently pass URLs in image bindings and cache locally!
     */
    public class ImageCacheConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage outImage = null;
            string url = value as string;

            Debug.WriteLine("starting conversion");

            //make sure the URL we got is actually a full URL
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                Debug.WriteLine("\tgot a bad uri [" + url + "], aborting conversion");
                return null;
            }

            //figure out if the URL is cached locally
            outImage = ImgCache.GetImageByURL(url);

            //if so, return that image
            //if not, get it from the web, save it, then return that
            if (outImage == null)
            {
                Debug.WriteLine("\timage not found in cache, caching now");
                outImage = ImgCache.RetrieveAndCacheByURL(url);
                
                //outImage = ImgCache.GetImageByURL(url);
                //outImage = new BitmapImage(new Uri(url, UriKind.Absolute));
            }
            else
                Debug.WriteLine("\timage found in cache!");

            return outImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
