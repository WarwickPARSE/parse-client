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

//OpenTK Imports
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;

using Microsoft.Kinect;

/*NOTE: Scanner modeller is making use of 2 gl's. This will be refactored to 1.
        For now, they are incompatible and any additions/deletions
        should maintain clear seperability between the commented blocks.*/

namespace PARSE
{
 
    class ScannerModeller
    {

        //Prototype specific definitions
        private short[]                z;
        private int                     x;
        private int                     y;

        //Modelling definitions
        private MeshGeometry3D          baseModel;
        private GeometryModel3D         baseModelProperties;
        private BitmapSource            bs;
        private short[]                 depthpixeldata;
        private byte[]                  depthFrame;
        private byte[]                  colorpixeldata;
        private byte[]                  colorFrame;

        //OpenTK Definitions
        private MeshGeometry3D          pcloud;

        //Prototype Constructor
        public ScannerModeller(Int16[] depthCollection, int xPoint, int yPoint, MeshGeometry3D glc)
        {
            this.x = xPoint;
            this.y = yPoint;
            this.z = depthCollection;
            this.pcloud = glc;
            baseModel = new MeshGeometry3D();
            baseModelProperties = new GeometryModel3D();

        }

        //Start of OpenTK 3D Methods

        public void glc_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw a little yellow triangle
            GL.Color3(System.Drawing.Color.Yellow);
           
        }

        //End of OpenTK 3D Methods

        //Start of WPF 3D Methods
        public void ClearViewport(Viewport3D mainViewport)
        {
            ModelVisual3D m;
            for (int i = mainViewport.Children.Count - 1; i >= 0; i--)
            {
                m = (ModelVisual3D)mainViewport.Children[i];
                if (m.Content is DirectionalLight == false)
                    mainViewport.Children.Remove(m);
            }
        }

        //Start of alternative point clouding method

        public void RenderKinectPoints()
        {
            Point3DCollection points = ViewportPlotter();
            CreatePointCloud(points);
        }

        private void CreatePointCloud(Point3DCollection p3d)
        {

            for (int i = 0; i < p3d.Count; i++)
            {
                RenderMesh(this.pcloud, p3d[i], 0.005);
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

            for (int i = 1; i < 640; i = i + 1) {

                for (int p = 1; p < 480; p = p + 1) {
                    
                    x = c1 / 640;
                    y = c2 / 480;

                    if (this.z[i*p]!=1) {
                        tempPoints.Add(new Point3D(x, y, (double) this.z[i*p]/8000));
                    }
                    //System.Diagnostics.Debug.WriteLine(x + " - " + y + " - " + (double) this.z[i*p]/8000);

                    c2++;
                }

                c1++;
                c2 = 0;
            } 


           /*
            double maxTheta = Math.PI * 2;
            double minY = -1.0;
            double maxY = 1.0;

            double dt = maxTheta / 70;
            double dy = (maxY - minY) / 70;

            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection points = new Point3DCollection();

            for (int yi = 0; yi <= 70; yi++)
            {
                double y = minY + yi * dy;

                for (int ti = 0; ti <= 70; ti++)
                {
                    double t = ti * dt;
                    double r = Math.Sqrt(1 - y * y);
                    double x = r * Math.Cos(t);
                    double z = r * Math.Sin(t);
                    tempPoints.Add(new Point3D(x, y, z));

                    System.Diagnostics.Debug.WriteLine(x + " - " + y + " - " + z);
                }
            } */

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

        //End of alternative point clouding method

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
