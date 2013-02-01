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

        public static void serialize(String filename, List<int[]> capcloud)
        {
            /*Serializes captured point cloud*/
            XmlSerializer serializer = new XmlSerializer(typeof(List<int[]>));
            TextWriter textWriter = new StreamWriter(filename);
            serializer.Serialize(textWriter, capcloud);
            textWriter.Close();
        }

        public static CloudVisualisation deserialize(String filename) {

            XmlSerializer deserializer = new XmlSerializer(typeof(List<int[]>));
            TextReader textReader = new StreamReader(filename);
            List<int[]> temp = (List<int[]>)(deserializer.Deserialize(textReader));
            CloudVisualisation cloud = new CloudVisualisation(temp);
            return cloud;
        }
        
    }
}
