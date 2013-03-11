using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.Library
{
    class PointCloud2
    {
        //width and height of a point cloud
        int height;
        int width;
        
        int point_step; //length of a point in bytes 
        int row_step;   //length of a row in bytes

        int[] data;     //the depth data

        bool is_dense;  //are all points valid?  
    }
}
