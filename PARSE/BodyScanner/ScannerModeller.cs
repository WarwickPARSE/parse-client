using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;


namespace PARSE
{
 
    class ScannerModeller
    {

        //Prototype specific definitions
        private Int16[]                 z;
        private int                     x;
        private int                     y;

        //Modelling definitions
        private MeshGeometry3D      baseModel;
        private GeometryModel3D     baseModelProperties;
        private short[]             depthpixeldata;
        private byte[]              depthFrame;
        private byte[]              colorpixeldata;
        private byte[]              colorFrame;

        //Prototype Constructor
        public ScannerModeller(Int16[] depthCollection, int xPoint, int yPoint)
        {
            this.x = xPoint;
            this.y = yPoint;
            this.z = depthCollection;
            baseModel = new MeshGeometry3D();
            baseModelProperties = new GeometryModel3D();

        }

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
      
        public ModelVisual3D run()
        {

            int pointsPlotted       = 0;
            Model3DGroup top        = new Model3DGroup();
            ModelVisual3D mod       = new ModelVisual3D();

            ModelVisual3D model = new ModelVisual3D();

            Model3DGroup topography = new Model3DGroup();
            Point3D[] points = GetRandomTopographyPoints();

            for (int z = 0; z <= 80; z = z + 10)
            {
                for (int x = 0; x < 9; x++)
                {
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z],
                                points[x + z + 10],
                                points[x + z + 1])
                    );
                    topography.Children.Add(
                        CreateTriangleModel(
                                points[x + z + 1],
                                points[x + z + 10],
                                points[x + z + 11])
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

        private Model3DGroup CreateTriangleModel(Point3D p0, Point3D p1, Point3D p2)
        {

            //Base mesh
            MeshGeometry3D mesh = new MeshGeometry3D();
            GeometryModel3D model = new GeometryModel3D();
            Model3DGroup gro = new Model3DGroup();
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

            //Temporary material
            Material material = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
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
