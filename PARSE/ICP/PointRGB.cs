using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE.ICP
{
    struct PointRGB
    {
        
        public PointRGB(Point3D point, double x, double y, double z) 
        {
            _x = x;
            _y = y;
            _z = z;
            _point = point; 
        }

        private double _x, _y, _z;
        private Point3D _point; 

        //todo: make the properties immutable 
        public double x {
            get { return _x; }
            set { _x = value; }
        }

        public double y
        {
            get { return _y; }
            set { _y = value; }
        }

        public double z
        {
            get { return _z; }
            set { _z = value; }
        }

        public Point3D point
        {
            get { return _point; }
            set { _point = value; }
        }
    }
}
