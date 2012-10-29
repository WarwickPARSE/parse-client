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
        private int                 z;
        private int                 x;
        private int                 y;

        //Modelling definitions
        private MeshGeometry3D      baseModel;
        private GeometryModel3D     baseModelProperties;
        private short[]             depthpixeldata;
        private byte[]              depthFrame;
        private byte[]              colorpixeldata;
        private byte[]              colorFrame;

        //Prototype Constructor
        public ScannerModeller(int realDepth, int xPoint, int yPoint)
        {
            this.x = xPoint;
            this.y = yPoint;
            this.z = realDepth;
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
            //Example geometry to be replaced by modelfactory from actual kinect data.
            baseModel.Positions.Add(new Point3D(-0.5, -0.5, 1));
            baseModel.Positions.Add(new Point3D(0.5, -0.5, 1));
            baseModel.Positions.Add(new Point3D(0.5, 0.5, 1));
            baseModel.Positions.Add(new Point3D(-0.5, 0.5, 1));
            // Back face
            baseModel.Positions.Add(new Point3D(-1, -1, -1));
            baseModel.Positions.Add(new Point3D(1, -1, -1));
            baseModel.Positions.Add(new Point3D(1, 1, -1));
            baseModel.Positions.Add(new Point3D(-1, 1, -1)); 

            //Example meshing to be replaced by modelfactory from actual kinect data.
            baseModel.TriangleIndices.Add(0);
            baseModel.TriangleIndices.Add(1);
            baseModel.TriangleIndices.Add(2);
            baseModel.TriangleIndices.Add(2);
            baseModel.TriangleIndices.Add(3);
            baseModel.TriangleIndices.Add(0);
   
            baseModel.TriangleIndices.Add(6);
            baseModel.TriangleIndices.Add(5);
            baseModel.TriangleIndices.Add(4);
            baseModel.TriangleIndices.Add(4);
            baseModel.TriangleIndices.Add(7);
            baseModel.TriangleIndices.Add(6);
   
            baseModel.TriangleIndices.Add(1);
            baseModel.TriangleIndices.Add(5);
            baseModel.TriangleIndices.Add(2);
            baseModel.TriangleIndices.Add(5);
            baseModel.TriangleIndices.Add(6);
            baseModel.TriangleIndices.Add(2);
   
            baseModel.TriangleIndices.Add(2);
            baseModel.TriangleIndices.Add(6);
            baseModel.TriangleIndices.Add(3);
            baseModel.TriangleIndices.Add(3);
            baseModel.TriangleIndices.Add(6);
            baseModel.TriangleIndices.Add(7);

            baseModel.TriangleIndices.Add(5);
            baseModel.TriangleIndices.Add(1);
            baseModel.TriangleIndices.Add(0);
            baseModel.TriangleIndices.Add(0);
            baseModel.TriangleIndices.Add(4);
            baseModel.TriangleIndices.Add(5);

            baseModel.TriangleIndices.Add(4);
            baseModel.TriangleIndices.Add(0);
            baseModel.TriangleIndices.Add(3);
            baseModel.TriangleIndices.Add(3);
            baseModel.TriangleIndices.Add(7);
            baseModel.TriangleIndices.Add(4);

            //create geometry
            baseModelProperties = new GeometryModel3D(baseModel, new DiffuseMaterial(Brushes.Gold));
            baseModelProperties.Transform = new Transform3DGroup();

            return baseModelProperties;
        }
    
    
    }

   
}
