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

        //Implementation Constructor
        public ScannerModeller(short[] pixelData, byte[] depthFrame, byte[] colorPixel, byte[] colorFrame)
        {
            //greg was here            

        }

        public GeometryModel3D run()
        {

            int pointsPlotted = 0;

            for (int i = 0; i < 640; i=i+1)
            {
                for (int p = 0; p < 480; p=p+1)
                {
                    baseModel.Positions.Add(new Point3D(i, p, this.z[(i*p)]));
                    pointsPlotted++;
                }

                pointsPlotted++;
            }

            //create geometry
            baseModelProperties = new GeometryModel3D(baseModel, new DiffuseMaterial(Brushes.Gold));
            baseModelProperties.Transform = new Transform3DGroup();
            System.Diagnostics.Debug.WriteLine(pointsPlotted);

            return baseModelProperties;
        }
    
    
    }

   
}
