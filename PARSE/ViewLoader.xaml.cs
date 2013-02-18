using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace PARSE
{
    /// <summary>
    /// Interaction logic for ViewLoader.xaml
    /// </summary>
    public partial class ViewLoader : Window
    {

        private KinectInterpreter kinectInterp;

        public ViewLoader()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(ViewLoader_Loaded);
        }

        private void ViewLoader_Loaded(object Sender, RoutedEventArgs e) {
            //place relative to coreloader
            this.Top = this.Owner.Top + 70;
            this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - this.Width - 20;
            //start kinectinterpreter
            kinectInterp = new KinectInterpreter(vpcanvas2);
        }

        protected override void OnInitialized(EventArgs e)
        {
 	        base.OnInitialized(e);
        }

        public void setLimbVisualisation()
        {
            //Streams for visualisation.
            kinectInterp.stopStreams();
            kinectInterp.startRGBStream();
            kinectInterp.startDepthStream();
            kinectInterp.startSkeletonStream();
            this.kinectInterp.kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);

            //set skeletal cues.
            this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);

            //track waist cue.


        }

        private void feed_SelectionChanged(object sender, RoutedEventArgs e)
        {
            string tmp = (selection.SelectedValue as ComboBoxItem).Content.ToString();

            if (tmp == "RGB")
            {
                kinectInterp.stopStreams();
                kinectInterp.startRGBStream();
                this.kinectInterp.kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(ColorImageReady);
            }
            else if (tmp == "RGB-D")
            {
                kinectInterp.stopStreams();
                kinectInterp.startDepthStream();
                this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
            }
            else if (tmp == "RGB Isolation")
            {
                Environment.Exit(1);
                kinectInterp.stopStreams();
                kinectInterp.startRGBStream();
                kinectInterp.startDepthStream();
                kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);
            }
            else if (tmp == "Depth Isolation")
            {
                kinectInterp.stopStreams();
                kinectInterp.startRGBStream();
                kinectInterp.startDepthStream();
                kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(SensorAllFramesReady);
                /*
                kinectInterp.stopStreams();
                kinectInterp.startDepthStream();
                kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
                 */
            }
            else if (tmp == "Skeleton")
            {
                kinectInterp.stopStreams();
                kinectInterp.startSkeletonStream();
                this.kinectInterp.kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(SkeletonFrameReady);
            }
        }

        private void SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            kinectInterp.SkeletonFrameReady(sender, e);
        }

        /// <summary>
        /// Kinect color polling method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>
        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            this.kinectImager.Source = kinectInterp.ColorImageReady(sender, e);
        }

        /// <summary>
        /// Kinect Depth Polling Method
        /// </summary>
        /// <param name="sender">originator of event</param>
        /// <param name="e">event ready identifier</param>
        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            this.kinectImager.Source = kinectInterp.DepthImageReady(sender, e);
        }

        private void SkeletonImageReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            kinectInterp.SkeletonFrameReady(sender, e);
        }

        private void SensorAllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            WriteableBitmap results = new WriteableBitmap();
            results = kinectInterp.SensorAllFramesReady(sender, e);
            //magic from bernie
            //this.kinectImager.Source = results[1];
            //this.kinectImager.OpacityMask = new ImageBrush { ImageSource = results[0] };
        }

    }
}
