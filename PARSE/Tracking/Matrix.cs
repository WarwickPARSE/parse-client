using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE.Tracking
{
    class Matrix
    {
        public double[,] value;

        public Matrix(int xx, int xy, int yx, int yy)
        {
            this.value = new double[2,2] { {xx, xy},{yx, yy} };
        }

        public Matrix()
        {
            this.value = new double[2, 2] { {0,0}, {0,0} };
        }

        public void add(Matrix input)
        {
            this.value[0, 0] += input.value[0, 0];
            this.value[0, 1] += input.value[0, 1];
            this.value[1, 0] += input.value[1, 0];
            this.value[1, 1] += input.value[1, 1];
        }

        public Matrix multiply(Matrix input)
        {
            Matrix result = new Matrix();

            return result;
        }

        public override String ToString()
        {
            return "Matrix= " + value[0, 0] + ", " + value[0, 1] + ", " + value[1, 0] + ", " + value[1, 1];
        }

        public void normalise()
        {
            double size = this.value[0, 0] * this.value[1, 1] - this.value[0, 1] * this.value[1, 0];
            this.value[0, 0] = this.value[0, 0] / size;
            this.value[0, 1] = this.value[0, 1] / size;
            this.value[1, 0] = this.value[1, 0] / size;
            this.value[1, 1] = this.value[1, 1] / size;
        }
    }
}
