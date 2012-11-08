using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Color;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Kinect;

namespace PARSE
{
    class ViewportCalibrator
    {

        private Canvas              vc;
        private DirectionalLight    d1;
        private PerspectiveCamera   pc;
        private Viewport3D          vp;

        public ViewportCalibrator(Canvas c)
        {
            this.vc = c;
        }

        public DirectionalLight setupDirectionalLights(Color col)
        {

            d1 = new DirectionalLight();

            d1.Color = col;
            d1.Direction = new Vector3D(1, 1, 1);

            return d1;

        }

        public PerspectiveCamera setupCamera()
        {

            pc = new PerspectiveCamera();

            //Kinect defaults
            pc.FarPlaneDistance = 8000;
            pc.NearPlaneDistance = 100;
            pc.FieldOfView = 10;
            pc.Position = new Point3D(160, 120, -1000);
            pc.LookDirection = new Vector3D(0, 0, 1);
            pc.UpDirection = new Vector3D(0, -1, 0);

            return pc;
        }

        public Viewport3D setupViewport(PerspectiveCamera p1, double height, double width)
        {

            vp = new Viewport3D();

            vp.IsHitTestVisible = false;
            vp.Camera = p1;
            vp.Height = height;
            vp.Width = width;

            return vp;
        }
    }
}
