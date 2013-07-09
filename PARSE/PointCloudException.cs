using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PARSE
{
    [Serializable()]
    public class PointCloudException : System.Exception
    {
        public PointCloudException() : base() { }
        public PointCloudException(string message) : base(message) { }
        public PointCloudException(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an 
        // exception propagates from a remoting server to the client.  
        protected PointCloudException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) { }
    }
}
