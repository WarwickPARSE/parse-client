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
            int noEntries = maxP.Count;                         //for ease of code readability, a waste of four bytes tbh 

            DenseVector tolX = (maxP - minP) / 1000;

            /* 
             * Make a uniform grid of points - used to sort points into local groups to speed up distance measurement
             */
            double spacing = (m1.ColumnCount ^ (1/6)) * Math.Sqrt(3);

            //convert the vectors into column marix representations 
            DenseMatrix maxPCol = (DenseMatrix)maxP.ToColumnMatrix();
            DenseMatrix minPCol = (DenseMatrix)minP.ToColumnMatrix();

            //determine the number of rows in these matrices 
            int noCols = maxPCol.RowCount;

            double spacingDistance = this.maxP(maxPCol - minPCol)[0]; //sketchy  

            //generate a new row matrix of normally spaced values 
            DenseVector xa = new DenseVector(noCols);
            DenseVector xb = new DenseVector(noCols);
            DenseVector xc = new DenseVector(noCols);
            
            //todo: if this doesn't work properly, then I need to have a look over the spacingDistance definition... 
            for (int i = 0; i < noCols; i++) {
                xa[i] = minPCol[0, 1] + i * spacingDistance;
                xb[i] = minPCol[0, 2] + i * spacingDistance;
                xc[i] = minPCol[0, 3] + i * spacingDistance;
            }

            //create a bunch of three dimensional matrices to hold the correspondence data - values gathered through empirical analysis
             

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

        /// <summary>
        /// An attempt at cloning the matlab ndgrid function, for all three vector inputs
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        public DenseMatrix ndgrid(DenseVector v1, DenseVector v2, DenseVector v3) {
            return null; 
        }

        /// <summary>
        /// Generates a 3d grid representation out of a set of three 
        /// same-sized vectors. A watered down version of ndgrid.
        /// </summary>
        /// <param name="m1">The first matrix</param>
        /// <param name="m2">The second matrix</param>
        /// <param name="m3">The third matrix</param>
        /// <returns>A tuple of three dimensional matrices</returns>
        private Tuple<double[, ,], double[, ,], double[, ,]> grid3d(DenseVector m1, DenseVector m2, DenseVector m3)
        {
            //hopefully the matrices are all the same size, otherwise this has already fallen over

            if (m1.Count == m2.Count && m2.Count == m3.Count) {
                //calculate dimensions of the matrix (this only works because they are dense!)
                int xDim, yDim, zDim;

                xDim = m1.Count;
                yDim = m2.Count;
                zDim = m3.Count;

                //generate three double "matrices" to hold our results
                double[, ,] x = new double[xDim, yDim, zDim];
                double[, ,] y = new double[xDim, yDim, zDim];
                double[, ,] z = new double[xDim, yDim, zDim];

                //the values have a gap and this depends on the number of rows (future use)
                int xGap, yGap, zGap;

                // x series. In this case the y value remains constant to the value of the position in the array
                for (int alpha = 0; alpha < m1.Count; alpha++) { 
                    //stick into all x and y indices
                    for (int i = 0; i < xDim; i++) {
                        for (int k = 0; k < zDim; k++) {
                            x[i, alpha, k] = m1[alpha];
                        }
                    }
                }

                // y series. For this the row value remains constant to the value of the position in the array
                for (int beta = 0; beta < m2.Count; beta++) { 
                    //stick into all y and z indices
                    for (int j = 0; j < yDim; j++) {
                        for (int k = 0; k < zDim; k++) {
                            y[beta, j, k] = m2[beta];
                        }
                    }
                }

                // z series. For this the depth value remains static (surprise)
                for (int gamma = 0; gamma < zDim; gamma++) { 
                    //stick in all x and y indices
                    for (int i = 0; i < xDim; i++) {
                        for (int j = 0; j < yDim; j++) {
                            z[i, j, gamma] = m3[gamma];
                        }
                    }
                }

                return new Tuple<double[, ,], double[, ,], double[, ,]>(x, y, z);
            }
            else { 
                //something terrible has happened! 
            }

            //pointless error double that will never be returned, if all goes to plan... else ahhh :(
            double[,,] e = new double[0,0,0];
            return new Tuple<double[, ,], double[, ,], double[, ,]>(e,e,e);
        }

        private DenseMatrix groupPoints() {
            return null; 
        }
    }

}
