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
 
    class ScannerModeller
    {

        //Prototype specific definitions
        private int[]                   z;
        private int                     x;
        private int                     y;
        private int                     s;

        //Modelling definitions
        private MeshGeometry3D          baseModel;
        private GeometryModel3D         baseModelProperties;
        private MeshGeometry3D          pcloud;

        //Constructor 1 definitions
        private Canvas                  viewportCanvas;
        private GeometryModel3D[]       pts;

        //Constructor 1
        public ScannerModeller(int[] depthCollection, int xPoint, int yPoint, MeshGeometry3D glc)
        {
            this.x = xPoint;
            this.y = yPoint;
            this.z = depthCollection;
            this.pcloud = glc;

            baseModel = new MeshGeometry3D();
            baseModelProperties = new GeometryModel3D();

        }
        //Constructor 2
        public ScannerModeller(Canvas g1, GeometryModel3D[] points)
        {
            this.viewportCanvas = g1;
            this.s = 4;
            this.pts = points;
        }

        /// <summary>
        /// RenderKinectPointsTriangle - renders kinect point cloud data using triangle meshes
        /// </summary>
        /// <returns>rendered point cloud model</returns>

        public GeometryModel3D[] RenderKinectPointsTriangle()
        {
            DirectionalLight    DirLight1 = new DirectionalLight();
            PerspectiveCamera   Camera1 = new PerspectiveCamera();
            Model3DGroup        modelGroup = new Model3DGroup();
            ModelVisual3D       modelsVisual = new ModelVisual3D();
            Viewport3D          myViewport = new Viewport3D();

            //directional light
            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(1, 1, 1);

            //camera specification
            Camera1.FarPlaneDistance = 8000;
            Camera1.NearPlaneDistance = 100;
            Camera1.FieldOfView = 10;
            Camera1.Position = new Point3D(160, 120, -1000);
            Camera1.LookDirection = new Vector3D(0, 0, 1);
            Camera1.UpDirection = new Vector3D(0, -1, 0);

            //create triangle mesh
            int i = 0;
            for (int y = 0; y < 480; y += s)
            {
                for (int x = 0; x < 640; x += s)
                {
                    pts[i] = Triangle(x, y, s);
                    pts[i].Transform = new TranslateTransform3D(0, 0, 0);
                    modelGroup.Children.Add(pts[i]);
                    i++;
                }
            }

            modelGroup.Children.Add(DirLight1);
            modelsVisual.Content = modelGroup;
            myViewport.IsHitTestVisible = false;
            myViewport.Camera = Camera1;
            myViewport.Children.Add(modelsVisual);
            viewportCanvas.Children.Add(myViewport);

            myViewport.Height = viewportCanvas.Height;
            myViewport.Width = viewportCanvas.Width;
            Canvas.SetTop(myViewport, 0);
            Canvas.SetLeft(myViewport, 0);

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

        //Start of pc method 2

        public GeometryModel3D RenderKinectPoints()
        {
            Point3DCollection points = ViewportPlotter();
            CreatePointCloud(points);

            return new GeometryModel3D(pcloud, new DiffuseMaterial(System.Windows.Media.Brushes.Red));
        }

        private void CreatePointCloud(Point3DCollection p3d)
        {

            GeometryModel3D[] points = new GeometryModel3D[640 * 480];
            GeometryModel3D sheet = new GeometryModel3D();

            sheet.Geometry = this.pcloud;
            sheet.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red));

            for (int i = 0; i < p3d.Count; i++)
            {
                RenderMesh(this.pcloud, p3d[i], 0.05);
            }

            this.pcloud.Freeze();
        
        }

        private Point3DCollection ViewportPlotter()
        {

            Point3DCollection tempPoints = new Point3DCollection();
            double x = 1;
            double y = 1;
            double c1 = 1;
            double c2 = 1;

            for (int i = 0; i < 640; i = i + 3) {

                for (int p = 0; p < 480; p = p + 3) {

                    x = c1;
                    y = c2;

                    if (this.z[((p*640)+i)] > 800 && this.z[((p*640)+i)] < 8000) {
                        tempPoints.Add(new Point3D(x, y, this.z[((p*640)+i)]));
                    }

                    c2 = c2 + 3;
                }

                c1 = c1 + 3;
                c2 = 0;
            }

            System.Diagnostics.Debug.WriteLine(tempPoints.Count());
            return tempPoints;
        }

        private void RenderMesh(MeshGeometry3D mg, Point3D center, double cdim)
        {

            int offset = mg.Positions.Count;

            if (mg != null)
            {
                mg.Positions.Add(new Point3D(center.X - cdim, center.Y + cdim, center.Z - cdim));
                mg.Positions.Add(new Point3D(center.X + cdim, center.Y + cdim, center.Z - cdim));
                mg.Positions.Add(new Point3D(center.X + cdim, center.Y + cdim, center.Z + cdim));
                mg.Positions.Add(new Point3D(center.X - cdim, center.Y + cdim, center.Z + cdim));
                mg.Positions.Add(new Point3D(center.X - cdim, center.Y - cdim, center.Z - cdim));
                mg.Positions.Add(new Point3D(center.X + cdim, center.Y - cdim, center.Z - cdim));
                mg.Positions.Add(new Point3D(center.X + cdim, center.Y - cdim, center.Z + cdim));
                mg.Positions.Add(new Point3D(center.X - cdim, center.Y - cdim, center.Z + cdim));

                mg.TriangleIndices.Add(offset + 3);
                mg.TriangleIndices.Add(offset + 2);
                mg.TriangleIndices.Add(offset + 6);

                mg.TriangleIndices.Add(offset + 3);
                mg.TriangleIndices.Add(offset + 6);
                mg.TriangleIndices.Add(offset + 7);

                mg.TriangleIndices.Add(offset + 2);
                mg.TriangleIndices.Add(offset + 1);
                mg.TriangleIndices.Add(offset + 5);

                mg.TriangleIndices.Add(offset + 2);
                mg.TriangleIndices.Add(offset + 5);
                mg.TriangleIndices.Add(offset + 6);

                mg.TriangleIndices.Add(offset + 1);
                mg.TriangleIndices.Add(offset + 0);
                mg.TriangleIndices.Add(offset + 4);

                mg.TriangleIndices.Add(offset + 1);
                mg.TriangleIndices.Add(offset + 4);
                mg.TriangleIndices.Add(offset + 5);

                mg.TriangleIndices.Add(offset + 0);
                mg.TriangleIndices.Add(offset + 3);
                mg.TriangleIndices.Add(offset + 7);

                mg.TriangleIndices.Add(offset + 0);
                mg.TriangleIndices.Add(offset + 7);
                mg.TriangleIndices.Add(offset + 4);

                mg.TriangleIndices.Add(offset + 7);
                mg.TriangleIndices.Add(offset + 6);
                mg.TriangleIndices.Add(offset + 5);

                mg.TriangleIndices.Add(offset + 7);
                mg.TriangleIndices.Add(offset + 5);
                mg.TriangleIndices.Add(offset + 4);

                mg.TriangleIndices.Add(offset + 2);
                mg.TriangleIndices.Add(offset + 3);
                mg.TriangleIndices.Add(offset + 0);

                mg.TriangleIndices.Add(offset + 2);
                mg.TriangleIndices.Add(offset + 0);
                mg.TriangleIndices.Add(offset + 1);
            
            }

        }
    
    }

   
}
