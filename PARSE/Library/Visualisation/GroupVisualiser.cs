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
    /*GroupVisualiser class 
     * Two display modes, one which display the point cloud as is using the point cloud object.
     * The other uses point cloud list constructor for a little bit more flexibility over colouring labelling. 
     */

    public class GroupVisualiser
    {

        //Constants for visualisation
        private int depthFrameWidth = 640;
        private int depthFrameHeight = 480;

        //Mesh geometry for visualisation computation.
        MeshGeometry3D mesh;
        List<PointCloud> clouds;

        //Geometry for visualisation computation.
        Point3D[] depthFramePoints;
        Point[] textureCoordinates;
        Vector3DCollection normals = new Vector3DCollection();
        List<int> triangleIndices = new List<int>();
        PARSE.ICP.PointRGB[] prgbs;
        Point3DCollection pcc;
        List<Point3DCollection> cloudList;

        public GroupVisualiser()
        {
            //parameterless
        }

        public GroupVisualiser(PointCloud pc)
        {
            //Constructor 1: construct point cloud as a single entity.

            mesh = new MeshGeometry3D();

            textureCoordinates = new Point[depthFrameHeight * depthFrameWidth];

            //get all points from the point cloud 
            prgbs = pc.getAllPoints();

            //resize the depthframe array (for efficiency)
            depthFramePoints = new Point3D[prgbs.Length];
        }

        public GroupVisualiser(List<PointCloud> pcs)
        {
            
            //Constructor 2: construct point cloud as a multiple scan entity.

            mesh = new MeshGeometry3D();

            this.clouds = pcs;

            textureCoordinates = new Point[depthFrameHeight * depthFrameWidth];
            depthFramePoints = new Point3D[depthFrameHeight * depthFrameWidth];

        }

        /*POINT CLOUD RENDERING*/

        public void preprocess()
        {
            if (this.clouds == null)
            {
                //preprocess condition for when we have a single point cloud to convert into point 3ds
                pcc = new Point3DCollection();

                //iterate over each point and stick it in depthframe points
                for (int i = 0; i < depthFramePoints.Length; i++)
                {
                    pcc.Add(prgbs[i].point);
                }

                createPointCloud(pcc);

                render();
            }
            else
            {
                //preprocess condition for when we have multiple point clouds to convert into point 3ds

                cloudList = new List<Point3DCollection>();

                //create a list of point 3d collections
                for (int i = 0; i < this.clouds.Count; i++)
                {
                    //iterate over each scan and grab points
                    Point3DCollection pcc = new Point3DCollection();
                    depthFramePoints = new Point3D[this.clouds[i].getAllPoints().Length];
                    ICP.PointRGB[] tmp = clouds[i].getAllPoints();

                    //jam into point cloud collection.
                    for (int j = 0; j < depthFramePoints.Length; j++)
                    {
                        pcc.Add(tmp[j].point);
                    }

                    System.Diagnostics.Debug.WriteLine("creating new pc's " + pcc.Count);

                    cloudList.Add(pcc);
                }

                render();

            }
        }

        public void render()
        {

            if (this.cloudList == null)
            {
                //Rendering method for gradient collection.
                GradientStopCollection gsc = new GradientStopCollection(4);
                gsc.Add(new GradientStop(Colors.Red, 0.1));
                gsc.Add(new GradientStop(Colors.Green, 0.2));
                gsc.Add(new GradientStop(Colors.Blue, 0.3));
                gsc.Add(new GradientStop(Colors.Orange, 0.4));

                runDemoModel();

                this.Model.Geometry = createMesh();
                this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new LinearGradientBrush(gsc, new Point(0, 0), new Point(100, 100)));
                //this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.LightSteelBlue));
                this.Model.Transform = new TranslateTransform3D(-1, -2, 1);

            }
            else
            {

                runDemoModel();

                for (int i = 0; i < this.cloudList.Count; i++)
                {

                    System.Diagnostics.Debug.WriteLine("Producing model " + i);

                    switch (i)
                    {
                        case 0:
                            createPointCloud(this.cloudList[0]);
                            this.Model.Geometry = createMesh();
                            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
                            this.Model.Transform = new TranslateTransform3D(-1, -2, 1);
                            this.armLabel.Geometry = createTextLabel("THIS IS AN ARM");


                            break;
                        case 1:
                            mesh = new MeshGeometry3D();
                            createPointCloud(this.cloudList[1]);
                            this.Model2.Geometry = createMesh();
                            this.Model2.Material = this.Model2.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Blue));
                            this.Model2.Transform = new TranslateTransform3D(-1, -2, 1);
                            break;
                        case 2:
                            mesh = new MeshGeometry3D();
                            createPointCloud(this.cloudList[2]);
                            this.Model3.Geometry = createMesh();
                            this.Model3.Material = this.Model3.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Green));
                            this.Model3.Transform = new TranslateTransform3D(-1, -2, 1);
                            break;
                        case 3:
                            mesh = new MeshGeometry3D();
                            createPointCloud(this.cloudList[3]);
                            this.Model4.Geometry = createMesh();
                            this.Model4.Material = this.Model4.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
                            this.Model4.Transform = new TranslateTransform3D(-1, -2, 1);
                            break;
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
            if (this.mesh == null) { System.Diagnostics.Debug.WriteLine("null"); }
            return this.mesh;
        }

        private void createPointCloud(Point3DCollection points)
        {

            for (int i = 0; i < points.Count; i++)
            {
                //AddCubeToMesh(pointCloudMesh, points[i], 0.005);
                addCubeToMesh(this.mesh, points[i], 0.002);
                //createAlternativeMesh();
            }
        }

        public MeshGeometry3D createAlternativeMesh()
        {

            //this.mesh is the relevant mesh that we are setting.

            //Define list of triangle indices over the whole model.
            var triangleIndices = new List<int>();

            //Define Vector3D normals.
            Vector3DCollection myNormalCollection = new Vector3DCollection();
            myNormalCollection.Add(new Vector3D(0, 0, 1));
            myNormalCollection.Add(new Vector3D(0, 1, 1));
            myNormalCollection.Add(new Vector3D(1, 1, 1));

            //iterate over each 3 points and create a triangle with the relevant indices.
            for (int i = 0; i < this.cloudList[0].Count-3; i+=3)
            {
                this.mesh.Positions.Add(this.cloudList[0][i]);
                this.mesh.Positions.Add(this.cloudList[0][i + 1]);
                this.mesh.Positions.Add(this.cloudList[0][i + 2]);

                //set triangle normals on the mesh.
                triangleIndices.Add(0);
                triangleIndices.Add(1);
                triangleIndices.Add(2);
            }

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(this.cloudList[0]),
                //TextureCoordinates = new System.Windows.Media.PointCollection(this.textureCoordinates),
                TriangleIndices = new Int32Collection(triangleIndices),
                Normals = myNormalCollection
            };

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

        public void setMaterial()
        {

            //TODO: Accept custom parameters for resetting material
            this.Model.Material = this.Model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
            this.Model2.Material = this.Model2.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
            this.Model3.Material = this.Model3.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));
            this.Model4.Material = this.Model4.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Orange));

        }

        /* TEXT LABEL RENDERING METHODS */

        public MeshGeometry3D createTextLabel(String label) //,position data?
        {
            TextBlock textblock = new TextBlock();
            DiffuseMaterial materialWithLabel = new DiffuseMaterial();

            //Set text parameters
            textblock.Text = label;
            textblock.Foreground = Brushes.Black; // setting the text color
            textblock.FontFamily = new FontFamily("Arial"); // setting the font to be used

            //Set brush parameters
            materialWithLabel.Brush = new VisualBrush(textblock);

            //Set width
            double width = label.Length * 30;

            //Positioning
            Point3D p0 = new Point3D(0,0,0) - width / 2 * new Vector3D(1,1,1) - 10 / 30 * new Vector3D(0.5,0.5,0.5);
            Point3D p1 = p0 + new Vector3D(0.5,0.5,0.5) * 1 * 30;
            Point3D p2 = p0 + new Vector3D(1,1,1) * width;
            Point3D p3 = p0 + new Vector3D(0.5,0.5,0.5) * 1 * 30 + new Vector3D(1,1,1) * width;

            //Create relevant mesh geometry.
            MeshGeometry3D mg = new MeshGeometry3D();
            mg.Positions = new Point3DCollection();
            mg.Positions.Add(p0);    // 0
            mg.Positions.Add(p1);    // 1
            mg.Positions.Add(p2);    // 2
            mg.Positions.Add(p3);    // 3

            mg.Positions.Add(p0);    // 4
            mg.Positions.Add(p1);    // 5
            mg.Positions.Add(p2);    // 6
            mg.Positions.Add(p3);    // 7

            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(3);
            mg.TriangleIndices.Add(1);
            mg.TriangleIndices.Add(0);
            mg.TriangleIndices.Add(2);
            mg.TriangleIndices.Add(3);

            mg.TriangleIndices.Add(4);
            mg.TriangleIndices.Add(5);
            mg.TriangleIndices.Add(7);
            mg.TriangleIndices.Add(4);
            mg.TriangleIndices.Add(7);
            mg.TriangleIndices.Add(6);

            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));
            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));

            mg.TextureCoordinates.Add(new Point(1, 1));
            mg.TextureCoordinates.Add(new Point(1, 0));
            mg.TextureCoordinates.Add(new Point(0, 1));
            mg.TextureCoordinates.Add(new Point(0, 0));

            return mg;
        }

        /* HELIX3D SANITY CHECKS/SETTERS/GETTERS*/

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

            if (cloudList != null)
            {
                this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
                this.Model2 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
                this.Model3 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
                this.Model4 = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
                this.armLabel = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
            }
            else
            {
                this.Model = new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(0, 0, 0), Material = greenMaterial, BackMaterial = greenMaterial };
            }
        }

        /// <summary>
        /// Gets or sets the sanity model.
        /// </summary>
        /// <value>The model.</value>
        
        //Point cloud getters/setters
        public GeometryModel3D Model { get; set; }
        public GeometryModel3D BaseModel { get; set; }
        public GeometryModel3D Model2 { get; set; }
        public GeometryModel3D Model3 { get; set; }
        public GeometryModel3D Model4 { get; set; }

        //Text label getters/setters
        public GeometryModel3D armLabel { get; set; }
        public GeometryModel3D legLabel { get; set; }
        public GeometryModel3D waistLabel { get; set; }
        public GeometryModel3D headLabel { get; set; }

    }
}