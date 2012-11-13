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
    class TriangularPointCloud
    {

        private Canvas                  cp;
        private GeometryModel3D[]       pts;
        private Model3DGroup            modelGroup;
        private ModelVisual3D           modelsVisual;
        private ViewportCalibrator      init;
        private int                     pos;

        public TriangularPointCloud(Canvas c, GeometryModel3D[] points)
        {
            this.cp = c;
            this.pts = points;
            this.pos = 4;
        }

        public GeometryModel3D[] render()
        {

            Model3DGroup modelGroup     = new Model3DGroup();
            Viewport3D visualisation    = new Viewport3D();
            ModelVisual3D modelsVisual  = new ModelVisual3D();
            int depthPoint              = 0;

            //Setup viewport environment
            init = new ViewportCalibrator(this.cp);
            visualisation = init.setupViewport(init.setupCamera(), this.cp.ActualHeight, this.cp.ActualWidth);

            //Create triangle mesh
            for (int y = 0; y < 480; y += pos)
            {
                for (int x = 0; x < 640; x += pos)
                {
                    pts[depthPoint] = Triangle(x, y, pos);
                    pts[depthPoint].Transform = new TranslateTransform3D(0, 0, 0);
                    modelGroup.Children.Add(pts[depthPoint]);
                    depthPoint++;
                }
            }

            //Add mesh to visualisation and viewport/canvas
            modelGroup.Children.Add(init.setupDirectionalLights(Colors.White));
            modelsVisual.Content = modelGroup;
            visualisation.Children.Add(modelsVisual);
            cp.Children.Add(visualisation);

            Canvas.SetTop(visualisation, 0);
            Canvas.SetLeft(visualisation, 0);

            return pts;   
        }

        private GeometryModel3D Triangle(double x, double y, double s)
        {
            Point3DCollection corners = new Point3DCollection();
            corners.Add(new Point3D(x, y, 0));
            corners.Add(new Point3D(x, y + s, 0));
            corners.Add(new Point3D(x + s, y + s, 0));
            corners.Add(new Point3D(x + y + s, x + y + s, 0));

            Int32Collection Triangles = new Int32Collection();
            Triangles.Add(0);
            Triangles.Add(1);
            Triangles.Add(2);
            Triangles.Add(3);

            MeshGeometry3D tmesh = new MeshGeometry3D();
            tmesh.Positions = corners;
            tmesh.TriangleIndices = Triangles;
            tmesh.Normals.Add(new Vector3D(0, 0, -1));

            GeometryModel3D msheet = new GeometryModel3D();
            msheet.Geometry = tmesh;
            msheet.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            return msheet;
        }
    }
}
