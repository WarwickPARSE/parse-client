using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace PARSE
{
    class ScanSerializer
    {

        public static void serialize(String filename, List<PointCloud> capcloud)
        {
            /*Serializes captured point cloud*/
            XmlSerializer serializer = new XmlSerializer(typeof(List<PointCloud>));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, capcloud);
            textWriter.Close();
        }

        public static CloudVisualisation deserialize(String filename) {

            XmlSerializer deserializer = new XmlSerializer(typeof(List<PointCloud>));
            TextReader textReader = new StreamReader(filename);
            List<PointCloud> temp = (List<PointCloud>)(deserializer.Deserialize(textReader));
            CloudVisualisation cloud = new CloudVisualisation(temp, false);
            return cloud;
        }
        
    }
}
