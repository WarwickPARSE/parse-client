using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;

namespace PARSE.ICP.Stitchers
{
    class AffineICP : Stitcher
    {
        List<PointCloud> pointClouds;
        List<PointCloud> txpointClouds;
        PointCloud pcd;

        /// <summary>
        /// Instantiates an empty point cloud, ready for us to dump some data
        /// </summary>
        public AffineICP() {
            //instantiate empty lists and point clouds 
            pointClouds = new List<PointCloud>();
            txpointClouds = new List<PointCloud>();
            pcd = new PointCloud();
        }

        /// <summary>
        /// Add a point cloud into the list of point clouds
        /// </summary>
        /// <param name="pc">Input point cloud</param>
        public override void add(PointCloud pc)
        {
            this.pointClouds.Add(pc);
        }

        /// <summary>
        /// Add a list of point clouds to the list of point clouds
        /// </summary>
        /// <param name="pcs"></param>
        public override void add(List<PointCloud> pcs)
        {
            this.pointClouds = pcs;
            Console.WriteLine("There are " + pcs.Count);
        }

        public override void stitch() {
            //todo: some form of datastructure to hold the transformation matrices

            /*
            //perform registration on each pair of clouds 
            for (int i = 0; i > pointClouds.Count -1; i++) { 
            
                //add this transform matrix to the previous one so that transformation is complete 
            }*/
        } 

        public void ICPStep(DenseMatrix m1, DenseMatrix m2) {
            double[] s = { 1, 1, 1, 0.01, 0.01, 0.01, 0.01, 0.01, 0.01 };                 //scale
            double[] p = { 0, 0, 0, 0, 0, 0, 100, 100, 100 };                           //parameters

            //jam the values into vectors so that we can multiply them and shiz
            DenseVector scale = new DenseVector(s);
            DenseVector parameters = new DenseVector(p);

            //distance error in final iteration 
            double fval_old = double.MaxValue;

            //change in distance error between two iterations 
            double fval_percep = 0;

            //matrix that contains the transformed points
            DenseMatrix pointsMoved = m2;

            //number of iterations 
            int itt = 0;

            //get the max and min points of the static points (operator divides only works over vectors for some reason >:( )
            DenseVector maxP = this.maxP(m1);
            DenseVector minP = this.minP(m1);

            DenseVector tolX = (maxP - minP) / 1000;

            double spacing = (m1.ColumnCount ^ (1/6)) * Math.Sqrt(3);

            //convert the vectors into column marix representations 
            DenseMatrix maxPCol = (DenseMatrix)maxP.ToColumnMatrix();
            DenseMatrix minPCol = (DenseMatrix)minP.ToColumnMatrix();

            DenseMatrix spacingDistance = (DenseMatrix)this.maxP(minPCol - maxPCol).ToColumnMatrix(); //sketchy  

        }
        

        //todo: should probably document this a bit... 
        private DenseMatrix mat_rot_3d(DenseVector r) {
            //convert to radians
            r = r * (Math.PI / 180);

            //these are defined because of my perceived speedup 
            //todo: this may be zero indexed, and therefore may need changing... 
            double sinr1 = Math.Sin(r[1]);
            double cosr1 = Math.Cos(r[1]);
            double sinr2 = Math.Sin(r[2]);
            double cosr2 = Math.Cos(r[2]);
            double sinr3 = Math.Sin(r[3]);
            double cosr3 = Math.Cos(r[3]);

            double[,] Rx = new double[,] {
                {1, 0,     0,        0},
                {0, cosr1, -(sinr1), 0},
                {0, sinr1, cosr1,    0},
                {0, 0,     0,        1}
            }; 

            double[,] Ry = new double[,] {
                {cosr2,    0, sinr2, 0},
                {0,        1, 0,     0},
                {-(sinr2), 0, cosr2, 0},
                {0,        0, 0,     1}
            };

            double[,] Rz = new double[,] {
                {cosr3, -(sinr3), 0, 0},
                {sinr3, cosr3,    0, 0},
                {0,     0,        1, 0},
                {0,     0,        0, 1}
            };

            //return the multiplication of these matrices 
            return new DenseMatrix(Rx) * new DenseMatrix(Ry) * new DenseMatrix(Rz); 
        }

        private DenseMatrix mat_siz_3d(double[] s)
        {
            double[,] m = new double[,] {
                {s[0], 0,    0,    0},
                {0,    s[1], 0,    0},
                {0,    0,    s[2], 0},
                {0,    0,    0,    1}
            };

            return new DenseMatrix(m);
        }

        private DenseMatrix mat_shear_3d(double[] h)
        {
            double[,] m = new double[,] {
                {1,    h[0], h[1], 0},
                {h[2], 1,    h[3], 0},
                {h[4], h[5], 1,    0},
                {0,    0,    0,    1}
            };

            return new DenseMatrix(m);
        }

        private DenseMatrix mat_tra_3d(double[] t) {
            double[,] m = new double[,] {
                {1,    0,    0,    t[0]},
                {0,    1,    0,    t[1]},
                {0,    0,    1,    t[2]},
                {0,    0,    0,    1}
            };

            return new DenseMatrix(m);
        } 

        /// <summary>
        /// Return the result of the stitching
        /// </summary>
        /// <returns>The result of the stitching</returns>
        public override PointCloud getResult()
        {
            return pcd;
        }

        public override List<PointCloud> getResultList()
        {
            return txpointClouds;
        }

        /// <summary>
        /// Finds the maximal column values for a matrix 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public DenseVector maxP(DenseMatrix m) {
            //maintain an array of colum max values
            double[] maxVals = new double[m.ColumnCount];

            //set every value to the min value
            for (int i = 0; i < maxVals.Length; i++) {
                maxVals[i] = double.MinValue;
            }

            //iterate over each column starting 
            for (int i = 0; i < m.ColumnCount; i++) {
                for (int j = 0; i < m.RowCount; j++) { 
                    //reassign the max value if the value being looked at is greater
                    if(maxVals[i] > m[i, j]) {
                        maxVals[i] = m[i, j];
                    }
                }
            }

            return new DenseVector(maxVals);
        }

        /// <summary>
        /// Finds the minimal column values for a matrix 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public DenseVector minP(DenseMatrix m) {
            //maintain an array of colum min values
            double[] minVals = new double[m.ColumnCount];

            //set every value to the max value
            for (int i = 0; i < minVals.Length; i++)
            {
                minVals[i] = double.MaxValue;
            }

            //iterate over each column starting 
            for (int i = 0; i < m.ColumnCount; i++)
            {
                for (int j = 0; i < m.RowCount; j++)
                {
                    //reassign the min value if the value being looked at is less 
                    if (minVals[i] < m[i, j])
                    {
                        minVals[i] = m[i, j];
                    }
                }
            }

            return new DenseVector(minVals);
        }
    }
}
