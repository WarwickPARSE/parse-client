using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace PARSE.ICP
{
    struct PointRGB
    {
        
        public PointRGB(Point3D point, double r, double g, double b) 
        {
            _r = r;
            _g = g;
            _b = b;
            _point = point; 
        }

        private double _r, _g, _b;
        private Point3D _point; 

        //todo: make the properties immutable 
        public double r {
            get { return _r; }
            set { _r = value; }
        }

        public double g
        {
            get { return _g; }
            set { _g = value; }
        }

        public double b
        {
            get { return _b; }
            set { _b = value; }
        }

        public Point3D point
        {
            get { return _point; }
            set { _point = value; }
        }
    }
}
