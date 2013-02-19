using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

using HelixToolkit.Wpf;

using Microsoft.Kinect;

namespace PARSE
{
    /*CloudVisualisation Class
     * - Displays a cloud visualisation based on the received point cloud structure
     * - Currently uses int arrays. Will use PointCloud type */

    public class GroupVisualiser
    {

        //Constants for visualisation
        private int depthFrameWidth = 640;
        private int depthFrameHeight = 480;

        MeshGeometry3D mesh;

        //Geometry for visualisation computation.
        Point3D[] depthFramePoints;
        Point[] textureCoordinates;
        Vector3DCollection normals = new Vector3DCollection();
        List<int> triangleIndices = new List<int>();
        PARSE.ICP.PointRGB[] prgbs;
        Point3DCollection pcc;

        public GroupVisualiser()
        {
            //parameterless
        }

        public GroupVisualiser(PointCloud pc)
        {
            mesh = new MeshGeometry3D();

            textureCoordinates = new Point[depthFrameHeight * depthFrameWidth];

            //get all points from the point cloud 
            prgbs = pc.getAllPoints();

            //resize the depthframe array (for efficiency)
            depthFramePoints = new Point3D[prgbs.Length];

        }

        public void preprocess()
        {
            pcc = new Point3DCollection();

            //iterate over each point and stick it in depthframe points
            for (int i = 0; i < depthFramePoints.Length; i++)
            {
                pcc.Add(prgbs[i].point);
            }

            createPointCloud(pcc);

            render();
        }

        public void render()
        {
            runDemoModel();

            this.Model.Geometry = createMesh();
            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
            this.Model.Transform = new TranslateTransform3D(-1, -2, 1);
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }


        public MeshGeometry3D createMesh()
        {
            return this.mesh;
        }

        /// <summary>
        /// Sanity check.
        /// </summary>
        public void runDemoModel()
        {
            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
            meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);

            //create a model
            this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
        }

        private void createPointCloud(Point3DCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                //AddCubeToMesh(pointCloudMesh, points[i], 0.005);
                addCubeToMesh(this.mesh, points[i], 0.002);
            }
        }

        /// <summary>
        /// Adds a cube to a given mesh with the centre at a given point
        /// </summary>
        /// <param name="mesh">A mesh to add the cube to</param>
        /// <param name="centre">Centre of the cube</param>
        /// <param name="size">Size of the cube</param>
        private void addCubeToMesh(MeshGeometry3D mesh, Point3D centre, double size)
        {
            if (mesh != null)
            {
                int offset = mesh.Positions.Count;

                //add triangle corners into the mesh 
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Y + size, centre.Z - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Y + size, centre.Z - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Y + size, centre.Z + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Y + size, centre.Z + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Y - size, centre.Z - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Y - size, centre.Z - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Y - size, centre.Z + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Y - size, centre.Z + size));

                //add triangle indices because WPF loves triangles  
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 6);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 1);
                mesh.TriangleIndices.Add(offset + 4);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 7);

                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 6);
                mesh.TriangleIndices.Add(offset + 5);

                mesh.TriangleIndices.Add(offset + 7);
                mesh.TriangleIndices.Add(offset + 5);
                mesh.TriangleIndices.Add(offset + 4);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 3);
                mesh.TriangleIndices.Add(offset + 0);

                mesh.TriangleIndices.Add(offset + 2);
                mesh.TriangleIndices.Add(offset + 0);
                mesh.TriangleIndices.Add(offset + 1);
            }
        }

        /// <summary>
        /// Gets or sets the sanity model.
        /// </summary>
        /// <value>The model.</value>
        public GeometryModel3D Model { get; set; }

    }
}