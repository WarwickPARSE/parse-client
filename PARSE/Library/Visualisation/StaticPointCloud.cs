using System;
using System.IO;
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

using HelixToolkit.Wpf;

using Microsoft.Kinect;

namespace PARSE
{
    class StaticPointCloud
    {

        //migrate helix viewport stuff later into viewportcalibrator

        public StaticPointCloud(BitmapSource parameters)
        {

            BitmapSource bitmapSource = parameters;

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(memoryStream);

            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();

            // Create a model group
            var modelGroup = new Model3DGroup();

            // Create a mesh builder and add a box to it
            var meshBuilder = new MeshBuilder(false, false);
            meshBuilder.AddBox(new Point3D(0, 0, 1), 1, 2, 0.5);
            meshBuilder.AddBox(new Rect3D(0, 0, 1.2, 0.5, 1, 0.4));

            // Create a mesh from the builder (and freeze it)
            var mesh = meshBuilder.ToMesh(true);

            // Create some materials
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var imageMaterial = MaterialHelper.CreateImageMaterial(bImg, 1);
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var insideMaterial = MaterialHelper.CreateMaterial(Colors.Yellow);

            // Add 3 models to the group (using the same mesh, that's why we had to freeze it)
            modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Material = imageMaterial, BackMaterial = imageMaterial });
            modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(-2, 0, 0), Material = redMaterial, BackMaterial = insideMaterial });
            modelGroup.Children.Add(new GeometryModel3D { Geometry = mesh, Transform = new TranslateTransform3D(2, 0, 0), Material = blueMaterial, BackMaterial = insideMaterial });
            //modelGroup.Children.Add(gm);

            // Set the property, which will be bound to the Content property of the ModelVisual3D (see MainWindow.xaml)
            this.Model = modelGroup;
        }


        //required components

        //imageMesh = new DiffuseMaterial(new ImageBrush(this.outputColorBitmap));
        //this.Model.Material = this.Model.BackMaterial = imageMesh;

        
                          /*  for (int a = 0; a < depthFrame.Height; a++)
                            {
                                for (int b = 0; b < depthFrame.Width; b++)
                                {
                                    this.textureCoordinates[a * depthFrame.Width + b]
                                        = new Point((double)b / (depthFrame.Width - 1), (double)a
                                            / (depthFrame.Height - 1));
                                }
                            }*/

                            //this.Model.Geometry = this.CreateMesh(depthFrame.Width, depthFrame.Height);

        /*   int tooNearDepth = depthStream.TooNearDepth;
           int tooFarDepth = depthStream.TooFarDepth;
           int unknownDepth = depthStream.UnknownDepth;

           int cx = depthStream.FrameWidth / 2;
           int cy = depthStream.FrameHeight / 2;

           double fxinv = 1.0 / 476;
           double fyinv = 1.0 / 476;

           double scale = 0.001;
      
                     if (visMode == 2)
               {

                   for(int iy = 0; iy < 480; iy++)
                   {
                       for (int ix = 0; ix < 640; ix++)
                       {
                           int i = (iy * 640) + ix;
                           this.rawDepth[i] = depthFrame[(iy * 640) + ix] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                           if (rawDepth[i] == unknownDepth || rawDepth[i] < tooNearDepth || rawDepth[i] > tooFarDepth)
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
                           }
                       }
                   }

               }*/

        /* private MeshGeometry3D CreateMesh(int width, int height, double depthDifferenceTolerance = 200)
         {
             var triangleIndices = new List<int>();
             for (int iy = 0; iy + 1 < height; iy++)
             {
                 for (int ix = 0; ix + 1 < width; ix++)
                 {
                     int i0 = (iy * width) + ix;
                     int i1 = (iy * width) + ix + 1;
                     int i2 = ((iy + 1) * width) + ix + 1;
                     int i3 = ((iy + 1) * width) + ix;

                     var d0 = this.rawDepth[i0];
                     var d1 = this.rawDepth[i1];
                     var d2 = this.rawDepth[i2];
                     var d3 = this.rawDepth[i3];

                     var dmax0 = Math.Max(Math.Max(d0, d1), d2);
                     var dmin0 = Math.Min(Math.Min(d0, d1), d2);
                     var dmax1 = Math.Max(d0, Math.Max(d2, d3));
                     var dmin1 = Math.Min(d0, Math.Min(d2, d3));

                     if (dmax0 - dmin0 < depthDifferenceTolerance && dmin0 != -1)
                     {
                         triangleIndices.Add(i0);
                         triangleIndices.Add(i1);
                         triangleIndices.Add(i2);
                     }

                     if (dmax1 - dmin1 < depthDifferenceTolerance && dmin1 != -1)
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
                 TriangleIndices = new Int32Collection(triangleIndices)
             };
         }*/

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public Model3D Model { get; set; }
       
    }
}
