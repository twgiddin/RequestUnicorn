using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace RequestUnicorn
{
    [DynamoDBTable("Rides")]
    public class Ride
    {
        [DynamoDBHashKey]
        public string RideId { get; set; }
        public DateTime RequestTime { get; set; }
        public Unicorn Unicorn { get; set; }
        public string UnicornName { get; set; }
        public string User { get; set; }
    }
}
