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
using Microsoft.Phone.Net.NetworkInformation;

namespace iFixit7
{
    public partial class FullscreenImage : PhoneApplicationPage
    {
        public string SourceURI { get; set; }
        
        public FullscreenImage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            this.SourceURI = this.NavigationContext.QueryString["ImgURI"];
            Debug.WriteLine("got image url id = " + SourceURI);

            if (SourceURI == "")
                NavigationService.GoBack();

            //make sure the image is in the cache or we are online
            if (!DeviceNetworkInformation.IsNetworkAvailable && ImgCache.GetImageByURL(SourceURI) == null)
            {
                MessageBox.Show("There is no network connection, and the selected image is not cached for offline viewing.");
                NavigationService.GoBack();
            }

            this.DataContext = this;
        }

        /*
         * Replace with http://msdn.microsoft.com/en-us/library/ff426933.aspx ?
         */

        /*
         * Taken almost entirely from:
         * http://www.codeproject.com/Articles/335069/Nice-panning-and-zooming-in-Windows-Phone-7
         */
        private double _imageScale = 1d;
        private Point _imageTranslation = new Point(0, 0);
        private Point _fingerOne;
        private Point _fingerTwo;
        private double _previousScale;

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            _fingerOne = e.GetPosition(MyImage, 0);
            _fingerTwo = e.GetPosition(MyImage, 1);
            _previousScale = 1;
        }

        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            var newScale = e.DistanceRatio / _previousScale;
            var currentFingerOne = e.GetPosition(MyImage, 0);
            var currentFingerTwo = e.GetPosition(MyImage, 1);
            var translationDelta = GetTranslationOffset(currentFingerOne,
            currentFingerTwo, _fingerOne, _fingerTwo, _imageTranslation, newScale);
            _fingerOne = currentFingerOne;
            _fingerTwo = currentFingerTwo;
            _previousScale = e.DistanceRatio;
            UpdatePicture(newScale, translationDelta);
        }

        private void UpdatePicture(double scaleFactor, Point delta)
        {
            var newscale = _imageScale * scaleFactor;
            var transform = (CompositeTransform)MyImage.RenderTransform;
            if (newscale > 1)
            {
                //ApplicationBar.IsVisible = false;
                _imageScale *= scaleFactor;
                _imageTranslation = new Point
                (_imageTranslation.X + delta.X, _imageTranslation.Y + delta.Y);
                transform.ScaleX = _imageScale;
                transform.ScaleY = _imageScale;
                transform.TranslateX = _imageTranslation.X;
                transform.TranslateY = _imageTranslation.Y;
            }
            else
            {
                //ApplicationBar.IsVisible = true;
                transform.TranslateX = 0;
                transform.TranslateY = 0;
                transform.ScaleX = transform.ScaleY = 1;
                _imageTranslation = new Point(0, 0);
            }
        }

        private Point GetTranslationOffset(Point currentFingerOne, Point currentFingerTwo,
        Point oldFingerOne, Point oldFingerTwo, Point currentPosition, double scale)
        {
            var newFingerOnePosition = new Point(
                currentFingerOne.X + (currentPosition.X - oldFingerOne.X) * scale,
                currentFingerOne.Y + (currentPosition.Y - oldFingerOne.Y) * scale);
            var newFingerTwoPosition = new Point(
                currentFingerTwo.X + (currentPosition.X - oldFingerTwo.X) * scale,
                currentFingerTwo.Y + (currentPosition.Y - oldFingerTwo.Y) * scale);
            var newPosition = new Point(
                (newFingerOnePosition.X + newFingerTwoPosition.X) / 2,
                (newFingerOnePosition.Y + newFingerTwoPosition.Y) / 2);
            return new Point(
                newPosition.X - currentPosition.X,
                newPosition.Y - currentPosition.Y);
        }

        private void PhoneApplicationPage_OrientationChanged
            (object sender, OrientationChangedEventArgs e)
        {
            if (e.Orientation == PageOrientation.Landscape ||
                e.Orientation == PageOrientation.LandscapeLeft ||
                e.Orientation == PageOrientation.LandscapeRight)
            {
                MyImage.Width = 720;
                MyImage.Height = 480;
            }
            else
            {
                MyImage.Width = 480;
                MyImage.Height = 720;
            }
        }

        private void GestureListener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            UpdatePicture(1.0, new Point(e.HorizontalChange, e.VerticalChange));
        }

        /*
         * When the image finishes loading, hide the progress indicator
         */
        private void TheImage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("image loaded");

            MyImage.Visibility = System.Windows.Visibility.Visible;
            LoadingBar.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}