using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.IO;

namespace PARSE
{
    class ScanSerializer
    {
        public static List<PointCloud> depthPc = new List<PointCloud>();

        public static void serialize(String filename, List<PointCloud> capcloud)
        {
            try
            {
                /*Extracts raw depth data from point cloud.*/
                List<int[]> depthPoints = new List<int[]>();

                for (int i = 0; i < capcloud.Count; i++)
                {
                    depthPoints.Add(capcloud[i].rawDepth);
                }

                /*Serializes captured point cloud*/
                XmlSerializer serializer = new XmlSerializer(typeof(List<int[]>));
                TextWriter textWriter = new StreamWriter(filename);
                serializer.Serialize(textWriter, depthPoints);
                textWriter.Close();
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }
        }

        public static CloudVisualisation deserialize(String filename) {

            try
            {
                /*Extracts raw depth data from serialization and creates pc struct*/

                XmlSerializer deserializer = new XmlSerializer(typeof(List<int[]>));
                TextReader textReader = new StreamReader(filename);
                List<int[]> temp = (List<int[]>)(deserializer.Deserialize(textReader));

                /*Deserializes*/
                for (int i = 0; i < temp.Count; i++)
                {
                    depthPc.Add(new PointCloud(null,temp[i]));
                }

                CloudVisualisation cloud = new CloudVisualisation(depthPc, false);
                return cloud;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err);
            }

            return null;
        }
        
    }
}
