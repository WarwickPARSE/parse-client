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

/*NOTE: Scanner modeller is making use of 2 gl's. This will be refactored to 1.
        For now, they are incompatible and any additions/deletions
        should maintain clear seperability between the commented blocks.*/

namespace PARSE
{
 
    class ScannerModeller
    {

        //Prototype specific definitions
        private int[]                   z;
        private int                     x;
        private int                     y;

        //Modelling definitions
        private MeshGeometry3D          baseModel;
        private GeometryModel3D         baseModelProperties;
        private BitmapSource            bs;

        //OpenTK Definitions
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
            this.pts = points;
        }
        
        //Start of pc method 1

        public GeometryModel3D[] RenderKinectPointsTriangle()
        {
            DirectionalLight DirLight1 = new DirectionalLight();
            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(1, 1, 1);
            int s = 4;


            PerspectiveCamera Camera1 = new PerspectiveCamera();
            Camera1.FarPlaneDistance = 8000;
            Camera1.NearPlaneDistance = 100;
            Camera1.FieldOfView = 10;
            Camera1.Position = new Point3D(160, 120, -1000);
            Camera1.LookDirection = new Vector3D(0, 0, 1);
            Camera1.UpDirection = new Vector3D(0, -1, 0);

            Model3DGroup modelGroup = new Model3DGroup();

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
            ModelVisual3D modelsVisual = new ModelVisual3D();
            modelsVisual.Content = modelGroup;
            Viewport3D myViewport = new Viewport3D();
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

            for (int i = 1; i < 640; i = i + 5) {

                for (int p = 1; p < 480; p = p + 5) {
                    
                    x = c1 / 640;
                    y = c2 / 480;

                    if (this.z[p*i] > 1000 && this.z[p*i] < 4000) {
                        tempPoints.Add(new Point3D(x, y, 0));
                    }

                    c2 = c2 + 5;
                }

                c1 = c1 + 5;
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

        //Start of pc method 3

        private Point3D[] GetRandomTopographyPoints()
        {
            //create a 10x10 topography.
            Point3D[] points = new Point3D[100];
            Random r = new Random();
            double y;
            double denom = 1000;
            int count = 0;
            for (int z = 0; z < 10; z++)
            {
                for (int x = 0; x < 10; x++)
                {
                    System.Threading.Thread.Sleep(1);
                    y = Convert.ToDouble(r.Next(1, 999)) / denom;
                    points[count] = new Point3D(x, y, z);
                    count += 1;
                }
            }
            return points;
        } 

        /// <summary>
        /// Plots points into WPF Viewport
        /// </summary>
        /// <returns>Base model with associated properties.</returns>

        public ModelVisual3D run(BitmapSource rgbImage)
        {

            Model3DGroup top = new Model3DGroup();
            ModelVisual3D mod = new ModelVisual3D();

            ModelVisual3D model = new ModelVisual3D();

            Model3DGroup topography = new Model3DGroup();
            Point3D[] points = GetRandomTopographyPoints();

            bs = rgbImage;
            int f = 0;

            //Columns over mesh
            for (int z = 0; z <= 80; z = z + 10)
            {

                f++;

                //Rows over mesh
                for (int x = 0; x < 9; x++)
                {
                    //Stitches standard and opposing triangles together.
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z],
                                points[x + z + 10],
                                points[x + z + 1], x*25, f*25)
                    );
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z + 1],
                                points[x + z + 10],
                                points[x + z + 11], x*25, f*25)
                    );
                }
            }

            model.Content = topography;
            return model;
        }

        /// <summary>
        /// Triangle constructor for meshes
        /// </summary>
        /// <param name="po">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <returns>New triangle model for meshes.</returns>

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2, int squareColumn, int squareRow)
        {

            //Base mesh
            MeshGeometry3D      mesh = new MeshGeometry3D();
            GeometryModel3D     model = new GeometryModel3D();
            Model3DGroup        gro = new Model3DGroup();
            ImageBrush          img = new ImageBrush();
            BitmapSource        bs2 = new CroppedBitmap();
            //Base normal
            Vector3D normal = CalculateNormal(p0, p1, p2);

            //Triangle positions and indices
            mesh.Positions.Add(p0);
            mesh.Positions.Add(p1);
            mesh.Positions.Add(p2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);

            //Calculate normals
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);
            mesh.Normals.Add(normal);

            //RGB Brush cropped at each interval over the mesh
            img.ImageSource = bs;


            bs2 = new CroppedBitmap(bs, new Int32Rect(squareColumn, squareRow, 71, 53));

            //Map texture coordinates
            Material material = new DiffuseMaterial(new ImageBrush(bs2));
            PointCollection pcfucker = new PointCollection();
            pcfucker.Add(new System.Windows.Point(0, 1));
            pcfucker.Add(new System.Windows.Point(1, 1));
            pcfucker.Add(new System.Windows.Point(1, 0));
            pcfucker.Add(new System.Windows.Point(0, 0));

            mesh.TextureCoordinates = pcfucker;
            model = new GeometryModel3D(mesh, material);

            //Add to model group
            gro.Children.Add(model);

            return gro;
        }

        /// <summary>
        /// Calculates the normal for a single triangle mesh
        /// </summary>
        /// <param name="p0">Point 1</param>
        /// <param name="p1">Point 2</param>
        /// <param name="p2">Point 3</param>
        /// <returns>normal for triangle meshes</returns>

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            Vector3D v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            Vector3D v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);

            return Vector3D.CrossProduct(v0, v1);
        }
    
    }

   
}
