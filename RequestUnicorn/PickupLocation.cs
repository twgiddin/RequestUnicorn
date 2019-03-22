using System;
using System.Collections.Generic;
using System.Text;

namespace RequestUnicorn
{
    //The original Node.js code returns an object with a single attribut that is the the pickup location
    public class PickupLocationBody
    {
        public PickupLocation PickupLocation { get; set; }
    }
    //The model of the pickup location
    public class PickupLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
