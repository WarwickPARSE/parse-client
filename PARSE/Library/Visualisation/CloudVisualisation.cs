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

    public class CloudVisualisation
    {

        //Constants for visualisation
        private int     depthFrameWidth = 640;
        private int     depthFrameHeight = 480;
        private int     cx = 640 / 2;
        private int     cy = 480 / 2;
        private int     tooCloseDepth = 0;
        private int     tooFarDepth = 4095;
        private int     unknownDepth = -1;
        private double  scale = 0.001;
        private double  fxinv = 1.0 / 480;
        private double  fyinv = 1.0 / 480;
        private double  ddt = 200;

        //Geometry for visualisation
        //This is public to be accessed for XML serialization.s
        List<PointCloud>    clouds;
        bool?               texture;
        
        //Geometry for visualisation computation.
        int[]               rawDepth;
        Point3D[]           depthFramePoints;
        Point[]             textureCoordinates;
        Vector3DCollection  normals = new Vector3DCollection();

        public CloudVisualisation()
        {
            //parameterless
        }

        public CloudVisualisation(List<PointCloud> pc, bool? texture)
        {
            //this will be a singleton soon
            this.clouds = pc;
            this.texture = texture;

            System.Diagnostics.Debug.WriteLine(pc[0].getxMax());
            System.Diagnostics.Debug.WriteLine(pc[0].getyMax());
            System.Diagnostics.Debug.WriteLine(pc[0].getxMin());
            System.Diagnostics.Debug.WriteLine(pc[0].getyMin());
            System.Diagnostics.Debug.WriteLine(pc[0].getKDTree().numberOfNodes());

            textureCoordinates = new Point[depthFrameHeight * depthFrameWidth];
            depthFramePoints = new Point3D[depthFrameHeight * depthFrameWidth];

            render();

        }
        
        public void render()
        {
            //create depth coordinates

            for (int i = 0; i < this.clouds.Count; i++)
            {
                this.rawDepth = this.clouds[i].rawDepth;
                runDemoModel(i);
                createDepthCoords();

                System.Diagnostics.Debug.WriteLine(this.clouds[i].rawDepth.Length);

                switch (i)
                {
                    case 0:
                        this.Model.Geometry = createMesh();

                        if (texture == false)
                        {
                            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
                        }
                        else
                        {
                            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new ImageBrush(this.clouds[i].bs));
                        }
                        
                        this.Model.Transform = new TranslateTransform3D(-1, -2, 1);
                        break;
                    case 1:
                        this.Model2.Geometry = createMesh();

                        if (texture == false)
                        {
                            this.Model2.Material = this.Model2.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
                        }
                        else
                        {
                            this.Model2.Material = this.Model2.BackMaterial = new DiffuseMaterial(new ImageBrush(this.clouds[i].bs));
                        }

                        //translate then rotate
                        Transform3DGroup transGroup = new Transform3DGroup();

                        TranslateTransform3D transTrans = new TranslateTransform3D(-0.50, -3.40, 1);
                        transGroup.Children.Add(transTrans);

                        RotateTransform3D transRotate = new RotateTransform3D();
                        AxisAngleRotation3D transRotateAxis = new AxisAngleRotation3D();

                        transRotateAxis.Axis = new Vector3D(0, 0, 1);
                        transRotateAxis.Angle = -90;
                        transRotate.Rotation = transRotateAxis;

                        transGroup.Children.Add(transRotate);

                        this.Model2.Transform = transGroup;

                        break;
                    case 2:
                        this.Model3.Geometry = createMesh();

                        if (texture == false)
                        {
                            this.Model3.Material = this.Model3.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
                        }
                        else
                        {
                            this.Model3.Material = this.Model3.BackMaterial = new DiffuseMaterial(new ImageBrush(this.clouds[i].bs));
                        }
                        //translate then rotate
                        Transform3DGroup transGroup2 = new Transform3DGroup();

                        TranslateTransform3D transTrans2 = new TranslateTransform3D(0.85, -2.95, 1);
                        transGroup2.Children.Add(transTrans2);

                        RotateTransform3D transRotate2 = new RotateTransform3D();
                        AxisAngleRotation3D transRotateAxis2 = new AxisAngleRotation3D();

                        transRotateAxis2.Axis = new Vector3D(0, 0, 1);
                        transRotateAxis2.Angle = 180;
                        transRotate2.Rotation = transRotateAxis2;

                        transGroup2.Children.Add(transRotate2);

                        this.Model3.Transform = transGroup2;
                        break;
                    case 3:
                        this.Model4.Geometry = createMesh();

                        if (texture == false)
                        {
                            this.Model4.Material = this.Model4.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
                        }
                        else
                        {
                            this.Model4.Material = this.Model4.BackMaterial = new DiffuseMaterial(new ImageBrush(this.clouds[i].bs));
                        }

                        //translate then rotate
                        Transform3DGroup transGroup3 = new Transform3DGroup();

                        TranslateTransform3D transTrans3 = new TranslateTransform3D(0.30, -1.50, 1);
                        transGroup3.Children.Add(transTrans3);

                        RotateTransform3D transRotate3 = new RotateTransform3D();
                        AxisAngleRotation3D transRotateAxis3 = new AxisAngleRotation3D();

                        transRotateAxis3.Axis = new Vector3D(0, 0, 1);
                        transRotateAxis3.Angle = 90;
                        transRotate3.Rotation = transRotateAxis3;

                        transGroup3.Children.Add(transRotate3);

                        this.Model4.Transform = transGroup3;

                        break;
                } 

            }

        }

        public void createDepthCoords()
        {

            int vertexCounter = 0;
            List<Point3D> vertexMaintainer = new List<Point3D>();

            for (int iy = 0; iy < 480; iy++)
            {
                for (int ix = 0; ix < 640; ix++)
                {
                    int i = (iy * 640) + ix;

                    if (rawDepth[i] == unknownDepth || rawDepth[i] < tooCloseDepth || rawDepth[i] > tooFarDepth)
                    {
                        this.rawDepth[i] = -1;
                        this.depthFramePoints[i] = new Point3D();
                    }
                    else
                    {
                        double zz = this.rawDepth[i] * scale;
                        double x = (cx - ix) * zz * fxinv;
                        double y = zz;
                        double z = (cy - iy) * zz * fyinv;
                        this.depthFramePoints[i] = new Point3D(x, y, z);

                        if (vertexCounter != 2)
                        {
                            vertexMaintainer.Add(new Point3D(x, y, z));
                            vertexCounter++;
                        }
                        else
                        {
                            vertexMaintainer.Add(new Point3D(x, y, z));
                            this.normals.Add(CalculateNormal(vertexMaintainer[0], vertexMaintainer[1], vertexMaintainer[2]));
                            vertexMaintainer.Clear();
                            vertexCounter = 0;
                        }
                       
                    }
                }
            }
        }

        private Vector3D CalculateNormal(Point3D p0, Point3D p1, Point3D p2)
        {
            var v0 = new Vector3D(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z);
            var v1 = new Vector3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            return Vector3D.CrossProduct(v0, v1);
        }


        public MeshGeometry3D createMesh()
        {
            var triangleIndices = new List<int>();

            Vector3DCollection myNormalCollection = new Vector3DCollection();
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 1, 1));
            myNormalCollection.Add(new Vector3D(1, 1, 1));

            for (int iy = 0; iy + 1 < depthFrameHeight; iy++)
            {
                for (int ix = 0; ix + 1 < depthFrameWidth; ix++)
                {
                    int i0 = (iy * depthFrameWidth) + ix;
                    int i1 = (iy * depthFrameWidth) + ix + 1;
                    int i2 = ((iy + 1) * depthFrameWidth) + ix + 1;
                    int i3 = ((iy + 1) * depthFrameWidth) + ix;

                    var d0 = this.rawDepth[i0];
                    var d1 = this.rawDepth[i1];
                    var d2 = this.rawDepth[i2];
                    var d3 = this.rawDepth[i3];

                    var dmax0 = Math.Max(Math.Max(d0, d1), d2);
                    var dmin0 = Math.Min(Math.Min(d0, d1), d2);
                    var dmax1 = Math.Max(d0, Math.Max(d2, d3));
                    var dmin1 = Math.Min(d0, Math.Min(d2, d3));

                    if (dmax0 - dmin0 < ddt && dmin0 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i1);
                        triangleIndices.Add(i2);
                    }

                    if (dmax1 - dmin1 < ddt && dmin1 != -1)
                    {
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i2);
                        triangleIndices.Add(i3);

                    }

                }
            }

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(this.depthFramePoints),
                TextureCoordinates = new System.Windows.Media.PointCollection(this.textureCoordinates),
                TriangleIndices = new Int32Collection(triangleIndices),
                Normals = this.normals

            };
        }

        /// <summary>
        /// Sanity check.
        /// </summary>

        public void runDemoModel(int pos)
        {

            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
            meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);

            switch(pos) {

                case 0: this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 1: this.Model2 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 2: this.Model3 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
                case 3: this.Model4 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial }; break;
            }

        }

        /// <summary>
        /// Gets or sets the sanity model.
        /// </summary>
        /// <value>The model.</value>

        public GeometryModel3D Model { get; set; }
        public GeometryModel3D BaseModel { get; set; }
        public GeometryModel3D Model2 { get; set; }
        public GeometryModel3D Model3 { get; set; }
        public GeometryModel3D Model4 { get; set; }

    }
}
