using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace PARSE.ICP
{
    class DenseMatrix3D
    {
        DenseVector[][] data;
         
        /// <summary>
        /// Constructs a dense 3-dimensional matrix with specified x, y and z coords
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="z">z coordinate</param>
        public DenseMatrix3D(int x, int y, int z) { 
            //data = new DenseVector[x][y];
        }

    }
}
