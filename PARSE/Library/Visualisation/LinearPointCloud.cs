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

using HelixToolkit.Wpf;

using Microsoft.Kinect;

namespace PARSE
{
    class LinearPointCloud
    {

        private GeometryModel3D cloud;

        //migrate helix viewport stuff later into viewportcalibrator

        public LinearPointCloud(GeometryModel3D mod)
        {
            this.cloud = mod;
        }

        public MeshGeometry3D render(int width, int height, GeometryModel3D mesh, Point3D[] pt, System.Windows.Point[] tx, int[] rawdepth, double depthDiff = 200)
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

                    var d0 = rawdepth[i0];
                    var d1 = rawdepth[i1];
                    var d2 = rawdepth[i2];
                    var d3 = rawdepth[i3];

                    var dmax0 = Math.Max(Math.Max(d0, d1), d2);
                    var dmin0 = Math.Min(Math.Min(d0, d1), d2);
                    var dmax1 = Math.Max(d0, Math.Max(d2, d3));
                    var dmin1 = Math.Min(d0, Math.Min(d2, d3));

                    if (dmax0 - dmin0 < depthDiff && dmin0 != -1)
                    {
                     
                        triangleIndices.Add(i0);
                        triangleIndices.Add(i1);
                        triangleIndices.Add(i2);

                    }

                    if (dmax1 - dmin1 < depthDiff && dmin1 != -1)
                    {

                        triangleIndices.Add(i0);
                        triangleIndices.Add(i2);
                        triangleIndices.Add(i3);
                    
                    }
                }

            }

            return new MeshGeometry3D()
            {
                Positions = new Point3DCollection(pt),
                TextureCoordinates = new PointCollection(tx),
                TriangleIndices = new Int32Collection(triangleIndices)
         
            };
        }

    }
}
