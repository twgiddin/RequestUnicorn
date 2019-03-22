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
    //Decorate the class with the table name so that DynamoDB knows which table to use
    [DynamoDBTable("Rides")]
    public class Ride
    {
        //Decorate the HashKey for DynamoDB
        [DynamoDBHashKey]
        public string RideId { get; set; }
        public DateTime RequestTime { get; set; }
        public Unicorn Unicorn { get; set; }
        //If the attribute name doesn't match the DynamoDB column name you could do this to have the maping work
        //[DynamoDBProperty("UnicornFirstName")
        public string UnicornName { get; set; }
        //You can also use the [DynamoDBIgnore] for columns you don't want stored in the DynamoDB Table
        public string User { get; set; }
    }
}
