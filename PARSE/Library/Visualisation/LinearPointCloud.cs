using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace PARSE
{
    class LinearPointCloud
    {

        private Canvas cp;
        private GeometryModel3D[] pts;
        private Model3DGroup modelGroup;
        private ModelVisual3D modelsVisual;
        private ViewportCalibrator init;

        public LinearPointCloud(Canvas c, GeometryModel3D[] points)
        {
            this.cp = c;
            this.pts = points;
        }



    }
}
