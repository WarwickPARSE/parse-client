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
    class PlaneVisualisation
    {

        MeshGeometry3D planeMesh;
        Point3D[] planePoint;
        Point3DCollection pointCoordinates;

        public PlaneVisualisation()
        {
            //parameterless constructor in case of serialization
        }

        public PlaneVisualisation(double[] x, double [] z)
        {
            //Two dimensional plot in 3d space, z is plotted as if it is y, depth value set to 1
            //Camera is fixed in position, viewport manipulation is disabled.

            planeMesh = new MeshGeometry3D();
            planePoint = new Point3D[x.Length];

            pointCoordinates = new Point3DCollection();

            //iterate over each point and jam it into point 3d's and the collection

            for (int i = 0; i < x.Length; i++)
            {
                pointCoordinates.Add(new Point3D(x[i]+1, z[i]-2.75, -1));
            }

            //create point cloud with the relevant co-ordinates
            createPointCloud(pointCoordinates);

            //render model with default texture.
            render();
        }

        private void createPointCloud(Point3DCollection points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                //AddCubeToMesh(pointCloudMesh, points[i], 0.005);
                addCubeToMesh(this.planeMesh, points[i], 0.005);
            }
        }

        public void render()
        {
            runDemoModel();

            this.Model.Geometry = createMesh();
            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
            this.Model.Transform = new TranslateTransform3D(-1, -2, 1);
        }

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

        public MeshGeometry3D createMesh()
        {
            return this.planeMesh;
        }

        private void addCubeToMesh(MeshGeometry3D mesh, Point3D centre, double size)
        {
            if (mesh != null)
            {
                int offset = mesh.Positions.Count;

                //add triangle corners into the mesh 
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Z + size, centre.Y - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Z + size, centre.Y - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Z + size, centre.Y + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Z + size, centre.Y + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Z - size, centre.Y - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Z - size, centre.Y - size));
                mesh.Positions.Add(new Point3D(centre.X + size, centre.Z - size, centre.Y + size));
                mesh.Positions.Add(new Point3D(centre.X - size, centre.Z - size, centre.Y + size));

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
